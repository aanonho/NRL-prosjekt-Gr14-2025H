using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Hent connection string (støtter både "Default" og "DefaultConnection")
var cs =
    builder.Configuration.GetConnectionString("Default") ??
    builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Mangler connection string 'Default' eller 'DefaultConnection' i appsettings/miljøvariabler.");

// EF Core + MySQL (Pomelo)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(cs, ServerVersion.AutoDetect(cs)));

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Auto-migrer database ved oppstart (nyttig i dev/Docker)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
