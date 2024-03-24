using CompanyApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompanyApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public required DbSet<Company> Companies { get; init; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var company = builder.Entity<Company>();
        company
            .Property(c => c.Name)
            .HasMaxLength(100);
        company
            .Property(c => c.TaxNumber)
            .HasMaxLength(10);
        company
            .Property(c => c.City)
            .HasMaxLength(50);
        company
            .Property(c => c.Street)
            .HasMaxLength(50);
        company
            .Property(c => c.HouseNumber)
            .HasMaxLength(10);
        company
            .Property(c => c.PostalCode)
            .HasMaxLength(10);
    }
    
    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken token = default)
    {
        foreach (var entity in ChangeTracker.Entries()
                     .Where(x => x is { Entity: Company, State: EntityState.Modified })
                     .Select(x => x.Entity)
                     .Cast<Company>())
        {
            entity.Updated = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, token);
    }
}