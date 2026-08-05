namespace CustomerApi;

using Microsoft.EntityFrameworkCore;

public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasOne<Customer>()
            .WithMany()
            .HasForeignKey(o => o.CustomerId);

        modelBuilder.Entity<Order>().HasKey(o => o.Id); // eplicit ID assignment
        modelBuilder.Entity<Customer>().HasKey(c => c.Id);
    }
}