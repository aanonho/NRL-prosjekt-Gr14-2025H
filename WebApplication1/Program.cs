// Denne filen setter opp hele ASP.NET Core-applikasjonen, fra konfigurasjon til ruting.
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataInfrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using WebApplication1.Services;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// Setter opp MVC og e-posttjenesten så UI og e-postvarsler fungerer fra start.
builder.Services.AddControllersWithViews();
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

// Leser inn e-postkonfig for eventuell videre bruk i appen.
var emailOptions = builder.Configuration.GetSection("EmailSettings").Get<EmailOptions>();

// Henter connection string fra miljøvariabel først, ellers fra config-fil
var connectionString =
    builder.Configuration.GetValue<string>("ConnectionStrings__DefaultConnection") ??
    builder.Configuration.GetConnectionString("DefaultConnection");

// Logger hvilken databasekobling som faktisk brukes slik at drift vet hva som skjer.
Console.WriteLine($"[Startup] Using connection string: {connectionString}");

// Setter opp EF Core mot MariaDB med eksplisitt versjon for forutsigbarhet.
var serverVersion = new MariaDbServerVersion(new Version(10, 11));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Konfigurerer cookie-basert autentisering og autorisasjon for hele appen.
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

// Lager HTTP-pipelinen med feilhåndtering, HTTPS, statiske filer og ruting.
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

// Standardrute for MVC-controllerne.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();