using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleOnlineStore_Dotnet.Models;

namespace SimpleOnlineStore_Dotnet.Data
{
    public class SOSContext : IdentityDbContext<User>
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public SOSContext(DbContextOptions<SOSContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>();

            builder.HasDefaultSchema("identity");
        }
    }
}
