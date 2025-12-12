using Microsoft.EntityFrameworkCore;
using OxideBrowserBack.Models.Portfolio;

namespace OxideBrowserBack.Data;

public class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options)
    {
    }

    public DbSet<PortfolioData> PortfolioData { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Experience> Experiences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PortfolioData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Projects)
                  .WithOne(p => p.PortfolioData)
                  .HasForeignKey(p => p.PortfolioDataId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Experiences)
                  .WithOne(e => e.PortfolioData)
                  .HasForeignKey(e => e.PortfolioDataId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Experience>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
