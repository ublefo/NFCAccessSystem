using NFCAccessSystem.Data;
using OtpNet;
using System.Security.Claims;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Authorization;

using var db = new AccessSystemContext();
Console.WriteLine($"Database path: {db.DbPath}.");

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddDbContext<AccessSystemContext>();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
    .AddBasic(options =>
    {
        options.Realm = "Basic Authentication";
        options.Events = new BasicAuthenticationEvents
        {
            OnValidateCredentials = context =>
            {
                bool sessionAuthSuccess = false;
                bool totpAuthSuccess = false;
                bool userIsAdmin = false;

                bool UserExists(int id)
                {
                    return (db.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
                }


                // get session user ID (db pk)
                if (!string.IsNullOrEmpty(context.HttpContext.Session.GetString("_UserId")))
                {
                    // if session is found
                    int currentSessionUserId = Convert.ToInt32(context.HttpContext.Session.GetString("_UserId"));
                    Console.WriteLine("Session found, session UserID: {0}", currentSessionUserId);
                    // check if user exists and is admin
                    User authenticatingUser = db.Users.FirstOrDefault(u => u.UserId == currentSessionUserId);

                    // if no user found, return
                    if (authenticatingUser == null)
                    {
                        Console.WriteLine("Basic auth (session flow): no user found.");
                        return Task.CompletedTask;
                    }

                    // check if user is admin
                    if (authenticatingUser.Admin)
                    {
                        userIsAdmin = authenticatingUser.Admin;
                        Console.WriteLine("Basic auth (session flow): is admin.");
                    }
                    else
                    {
                        Console.WriteLine("Basic auth (session flow): not admin.");
                    }

                    sessionAuthSuccess = true;
                    Console.WriteLine("Session Auth Success.");
                }
                else
                {
                    // if session is not found
                    Console.WriteLine("No session found, doing full auth flow.");
                    Console.WriteLine("ID: " + context.Username);
                    Console.WriteLine("TOTP Provided: " + context.Password);

                    // find user by UID
                    User authenticatingUser = db.Users.FirstOrDefault(u => u.TagUid == context.Username);

                    // if no user found, return
                    if (authenticatingUser == null)
                    {
                        Console.WriteLine("Basic auth (TOTP flow): no user found.");
                        return Task.CompletedTask;
                    }

                    // check if user is admin
                    if (authenticatingUser.Admin)
                    {
                        userIsAdmin = authenticatingUser.Admin;
                        Console.WriteLine("Basic auth (TOTP flow): is admin.");
                    }
                    else
                    {
                        Console.WriteLine("Basic auth (TOTP flow): not admin.");
                    }

                    // check TOTP
                    var totp = new Totp(Base32Encoding.ToBytes(authenticatingUser.TotpSecret));
                    long timeWindowUsed;

                    var totpVerified = totp.VerifyTotp(context.Password, out timeWindowUsed,
                        VerificationWindow.RfcSpecifiedNetworkDelay);

                    // enforce one time use
                    if (totpVerified && authenticatingUser.MostRecentTotp != context.Password)
                    {
                        // save verified TOTP to db
                        authenticatingUser.MostRecentTotp = context.Password;
                        db.Update(authenticatingUser);
                        db.SaveChanges();
                        totpAuthSuccess = true;
                    }
                    else
                    {
                        Console.WriteLine("Basic auth (TOTP flow): reused OTP!");
                    }

                    Console.WriteLine(totpAuthSuccess ? "TOTP auth success." : "TOTP auth failed.");
                }


                if (sessionAuthSuccess || totpAuthSuccess)
                {
                    Console.WriteLine("Auth Successful, setting claims.");
                    var userRole = userIsAdmin ? "Admin" : "User";
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, context.Username, ClaimValueTypes.String,
                            context.Options.ClaimsIssuer),
                        new Claim(ClaimTypes.Name, context.Username, ClaimValueTypes.String,
                            context.Options.ClaimsIssuer),
                        new Claim(ClaimTypes.Role, userRole, ClaimValueTypes.String, context.Options.ClaimsIssuer),
                    };

                    context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                    context.Success();
                }
                else
                {
                    Console.WriteLine("Basic auth: auth failed.");
                }

                Console.WriteLine("---------- Basic auth flow finished. ----------");

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "NFCAccessSystem.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{action=Index}/{id?}",
    defaults: new {controller = "Home"});

// TODO: test code, remove after testing
app.MapGet("/authenticate", [Authorize](ClaimsPrincipal user) => $"Hello {user!.Identity!.Name}");
app.MapGet("/unauthorized", () => Results.Unauthorized());

app.Run();