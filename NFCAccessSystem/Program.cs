using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using NFCAccessSystem.Data;
using NFCAccessSystem.Models;
using OtpNet;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Authorization;

using var db = new AccessSystemContext();
Console.WriteLine($"Database path: {db.DbPath}.");

var builder = WebApplication.CreateBuilder(args);
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
                // find user by UID
                User authenticatingUser = db.Users.FirstOrDefault(u => u.TagUid == context.Username);

                if (authenticatingUser == null)
                {
                    // if no user found just return
                    Console.WriteLine("Basic auth: no user found.");
                    return Task.CompletedTask;
                }

                var totp = new Totp(Base32Encoding.ToBytes(authenticatingUser.TotpSecret));
                long timeWindowUsed;

                Console.WriteLine("ID: " + context.Username);
                Console.WriteLine("TOTP Provided: " + context.Password);

                if (totp.VerifyTotp(context.Password, out timeWindowUsed, VerificationWindow.RfcSpecifiedNetworkDelay))
                {
                    Console.WriteLine("Auth Successful.");
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, context.Username, ClaimValueTypes.String,
                            context.Options.ClaimsIssuer),
                        new Claim(ClaimTypes.Name, context.Username, ClaimValueTypes.String,
                            context.Options.ClaimsIssuer)
                    };

                    context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                    context.Success();
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

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