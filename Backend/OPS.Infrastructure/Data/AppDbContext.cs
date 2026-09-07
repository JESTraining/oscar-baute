using Microsoft.EntityFrameworkCore;
using OPS.Domain.Common;
using OPS.Domain.Entities;

namespace OPS.Infrastructure.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<InventoryCheck> InventoryChecks { get; set; }
    public DbSet<User> Users { get; set; }
    
   protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(builder =>
        {
            builder.HasIndex(o => o.OrderNumber).IsUnique();
            builder.Property(o => o.OrderNumber).HasMaxLength(20).IsRequired();
            builder.Property(o => o.CustomerName).HasMaxLength(200).IsRequired();
            builder.Property(o => o.CustomerEmail).HasMaxLength(200);
            builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");

            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.InventoryChecks)
                .WithOne(ic => ic.Order)
                .HasForeignKey(ic => ic.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Order>().HasQueryFilter(x => !x.IsDeleted);
        
        modelBuilder.Entity<OrderItem>(builder =>
        {
            builder.Property(oi => oi.ProductName).HasMaxLength(200).IsRequired();
            builder.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
        });
        modelBuilder.Entity<OrderItem>().HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<User>(builder =>
        {
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Email).HasMaxLength(200).IsRequired();
            builder.Property(u => u.Password).HasMaxLength(300).IsRequired();
            builder.Property(u => u.Role).HasMaxLength(50).IsRequired();

            var seededAt = DateTimeOffset.Now;
            builder.HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@test.com",
                    Password = "Test@123",
                    Role = Roles.Admin,
                    CreatedAt = seededAt,
                    UpdatedAt = seededAt,
                    IsDeleted = false
                },
                new User
                {
                    Id = 2,
                    Email = "regularuser@test.com",
                    Password = "Test@123",
                    Role = Roles.RegularUser,
                    CreatedAt = seededAt,
                    UpdatedAt = seededAt,
                    IsDeleted = false
                }
            );
        });
        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }

}