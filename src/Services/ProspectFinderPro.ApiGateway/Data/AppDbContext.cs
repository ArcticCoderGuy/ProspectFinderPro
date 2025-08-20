using Microsoft.EntityFrameworkCore;
using ProspectFinderPro.ApiGateway.Models;

namespace ProspectFinderPro.ApiGateway.Data;

public class AppDbContext(DbContextOptions<AppDbContext> opts) : DbContext(opts)
{
    public DbSet<Company> Companies => Set<Company>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>()
            .Property(c => c.Turnover)
            .HasPrecision(18, 2);
    }
}
