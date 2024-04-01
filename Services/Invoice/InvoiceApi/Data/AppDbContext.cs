using InvoiceApi.Domain.Entities;
using InvoiceApi.Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public required DbSet<Item> Items { get; init; }
    public required DbSet<Invoice> Invoices { get; init; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        var item = builder.Entity<Item>();
        var invoice = builder.Entity<Invoice>();

        item.Property(i => i.Name).HasMaxLength(250);
        item.Property(i => i.Unit).HasConversion<string>();
        item.Property(i => i.Vat).HasConversion<string>();
        item.Property(i => i.NetPrice).HasColumnType("decimal(18,2)");
        item.Property(i => i.GrossPrice).HasColumnType("decimal(18,2)");

        invoice.Property(i => i.Number).HasMaxLength(150);
        invoice.Property(i => i.TotalNetPrice).HasColumnType("decimal(18,2)");
        invoice.Property(i => i.TotalGrossPrice).HasColumnType("decimal(18,2)");
        invoice.Property(i => i.PaymentType).HasConversion<string>();
        invoice.Property(i => i.Status).HasConversion<string>();

        invoice.HasMany(inv => inv.Items)
            .WithOne(ite => ite.Invoice)
            .HasForeignKey(ite => ite.InvoiceId);
    }
    
    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken token = default)
    {
        foreach (var entity in ChangeTracker.Entries()
                     .Where(x => x is { Entity: BaseAuditableEntity, State: EntityState.Modified })
                     .Select(x => x.Entity)
                     .Cast<BaseAuditableEntity>())
        {
            entity.Updated = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, token);
    }
}