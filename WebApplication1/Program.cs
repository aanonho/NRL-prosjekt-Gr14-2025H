using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataInfrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using WebApplication1.Services;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

var emailOptions = builder.Configuration.GetSection("EmailSettings").Get<EmailOptions>();

/*
//Henter connection string fra �appsettings.json� filen
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
*/

// Henter connection string fra miljøvariabel først, ellers fra config-fil
var connectionString =
    builder.Configuration.GetValue<string>("ConnectionStrings__DefaultConnection") ??
    builder.Configuration.GetConnectionString("DefaultConnection");

// Test to log which connection string is being used
Console.WriteLine($"[Startup] Using connection string: {connectionString}");


/*
// Entity Framework Core configuration with Mariadb
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
*/

// Using explicit version of MariaDB rather than AutoDetect
var serverVersion = new MariaDbServerVersion(new Version(10, 11));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
