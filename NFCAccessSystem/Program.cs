using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using NFCAccessSystem.Data;
using NFCAccessSystem.Models;
using OtpNet;

using var db = new AccessSystemContext();

// Note: This sample requires the database to be created before running.
Console.WriteLine($"Database path: {db.DbPath}.");

//db.Add(new User {TagUid = 0xaa55, TotpSecret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20))});
db.SaveChanges();

// // Read
// Console.WriteLine("Querying for a blog");
// var blog = db.Blogs
//     .OrderBy(b => b.BlogId)
//     .First();
// 
// // Update
// Console.WriteLine("Updating the blog and adding a post");
// blog.Url = "https://devblogs.microsoft.com/dotnet";
// blog.Posts.Add(
//     new Post {Title = "Hello World", Content = "I wrote an app using EF Core!"});
// db.SaveChanges();
// 
// // Delete
// Console.WriteLine("Delete the blog");
// db.Remove(blog);
// db.SaveChanges();
// 

//var users = db.Users.ToList();
//
//
//var key = KeyGeneration.GenerateRandomKey(20);
//
//var base32String = Base32Encoding.ToString(key);
//var base32Bytes = Base32Encoding.ToBytes(base32String);
//
//var otp = new Totp(base32Bytes);


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AccessSystemContext>();
// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();