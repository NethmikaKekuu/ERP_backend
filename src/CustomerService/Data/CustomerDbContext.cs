using CustomerService.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Data
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<CustomerOrderReference> CustomerOrderReferences { get; set; }
        public DbSet<CustomerOrderReferenceItem> CustomerOrderReferenceItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.Email).IsUnique(); // Email must be unique
                entity.Property(c => c.Email).IsRequired().HasMaxLength(255);
                entity.Property(c => c.FullName).IsRequired().HasMaxLength(255);
                entity.Property(c => c.PasswordHash).IsRequired();
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // CustomerAddress
            modelBuilder.Entity<CustomerAddress>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasOne(a => a.Customer)
                      .WithMany(c => c.Addresses)
                      .HasForeignKey(a => a.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CustomerOrderReference
            modelBuilder.Entity<CustomerOrderReference>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(o => o.Customer)
                      .WithMany(c => c.OrderReferences)
                      .HasForeignKey(o => o.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CustomerOrderReferenceItem
            modelBuilder.Entity<CustomerOrderReferenceItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(i => i.LineTotal).HasColumnType("decimal(18,2)");
                entity.HasOne(i => i.CustomerOrderReference)
                      .WithMany(o => o.Items)
                      .HasForeignKey(i => i.CustomerOrderReferenceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}