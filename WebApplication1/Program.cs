using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using WebApplication1.DataInfrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("ObstacleDb"));

// Add services to the container.
builder.Services.AddControllersWithViews();

//Henter connection string fra �appsettings.json� filen 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//Oppretter en instans av MySqlConnection 
builder.Services.AddSingleton(new MySqlConnection(connectionString));

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

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
