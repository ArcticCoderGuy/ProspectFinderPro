using Microsoft.EntityFrameworkCore;
using ProspectFinderPro.Shared.Models;

namespace ProspectFinderPro.Shared.Data;

public class ProspectFinderDbContext : DbContext
{
    public ProspectFinderDbContext(DbContextOptions<ProspectFinderDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<FinancialData> FinancialData { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<ProductOwnershipAnalysis> ProductOwnershipAnalyses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Company configuration
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BusinessId).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => new { e.Turnover, e.HasOwnProducts });
            entity.HasIndex(e => e.Industry);
            entity.HasIndex(e => e.Location);
            entity.HasIndex(e => e.ProductConfidenceScore);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CompanyId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsMainProduct);

            entity.HasOne(e => e.Company)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // FinancialData configuration
        modelBuilder.Entity<FinancialData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyId, e.Year }).IsUnique();
            entity.HasIndex(e => e.Year);

            entity.HasOne(e => e.Company)
                .WithMany(e => e.FinancialHistory)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // Contact configuration
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CompanyId);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.IsDecisionMaker);

            entity.HasOne(e => e.Company)
                .WithMany(e => e.Contacts)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // ProductOwnershipAnalysis configuration
        modelBuilder.Entity<ProductOwnershipAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CompanyId).IsUnique();
            entity.HasIndex(e => e.OverallConfidenceScore);

            entity.HasOne(e => e.Company)
                .WithOne(e => e.ProductOwnershipAnalysis)
                .HasForeignKey<ProductOwnershipAnalysis>(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.AnalysisDate)
                .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}