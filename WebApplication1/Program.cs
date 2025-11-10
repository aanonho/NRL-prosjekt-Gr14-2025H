using Microsoft.EntityFrameworkCore;
using WebApplication1.DataInfrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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

//  App DB 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Register dedicated Identity DbContext (contains AspNetUsers etc.)
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Identity (roles available for later, but we’re not using them now)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = true;

    // Strong password policy per your requirement
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<AuthDbContext>()   // <-- IMPORTANT: use AuthDbContext
.AddDefaultTokenProviders();

// 👇👇👇 NEW: dev email sender for the home “DEV Email Preview”
builder.Services.AddSingleton<WebApplication1.Services.IEmailSender, WebApplication1.Services.DevEmailSender>();

// 👇👇👇 NEW: Authentication/Authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

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

//  Auth middleware must be before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

//  Identity tables exist (applies AuthDbContext migrations if any)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var authDb = services.GetRequiredService<AuthDbContext>();
    await authDb.Database.MigrateAsync();
}

app.Run();
