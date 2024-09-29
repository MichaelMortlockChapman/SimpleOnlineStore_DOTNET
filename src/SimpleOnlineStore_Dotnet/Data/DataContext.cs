using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleOnlineStore_Dotnet.Models;

namespace SimpleOnlineStore_Dotnet.Data {
    public class DataContext : IdentityDbContext<User> {

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    }
}
