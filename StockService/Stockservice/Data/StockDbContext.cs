using Microsoft.EntityFrameworkCore;
using Stockservice.Models;

namespace Stockservice.Data;

public class StockDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<StockReservation> StockReservations { get; set; }

    public StockDbContext(DbContextOptions<StockDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(250);
            entity.Property(x => x.Balance).HasDefaultValue(0);
        });

        modelBuilder.Entity<StockReservation>(entity =>
        {
            entity.ToTable("StockReservations");
            entity.Property(x => x.Quantity).IsRequired();
            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
