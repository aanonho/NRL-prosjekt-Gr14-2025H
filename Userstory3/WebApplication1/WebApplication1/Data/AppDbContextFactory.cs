// Data/AppDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApplication1.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>();
            var cs = "Server=127.0.0.1;Port=3306;Database=nrl;User Id=nrl;Password=nrlpass;";
            var v = new MySqlServerVersion(new Version(8, 0, 34));
            opts.UseMySql(cs, v);
            return new AppDbContext(opts.Options);
        }
    }
}
