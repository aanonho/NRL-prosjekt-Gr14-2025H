// This file sets up the entire ASP.Net Core application, from configuration to routing.
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataInfrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using WebApplication1.Services;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// Sets up MVC and the email service so UI and email notifications work from the start.
builder.Services.AddControllersWithViews();
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

// Reading email configuration for potential further use in the app.
var emailOptions = builder.Configuration.GetSection("EmailSettings").Get<EmailOptions>();

// Getting connection string from an environment variable first, otherwise from the config file.
var connectionString =
    builder.Configuration.GetValue<string>("ConnectionStrings__DefaultConnection") ??
    builder.Configuration.GetConnectionString("DefaultConnection");

// Logs whish database connection is actually used so operations know what's happening.
Console.WriteLine($"[Startup] Using connection string: {connectionString}");

// Configures EF Core for MariaDB with explicit version for predictability.
var serverVersion = new MariaDbServerVersion(new Version(10, 11));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Configurerer cookie-based authentication and autorixation for the entire app.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Builds the HTTP-pipeline with error handling, HTTPS, static files, and routing.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Default route for MVC controllers.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
