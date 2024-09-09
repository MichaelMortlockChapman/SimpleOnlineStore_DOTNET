using Microsoft.EntityFrameworkCore;
using SimpleOnlineStore_Dotnet.Models;

namespace SimpleOnlineStore_Dotnet.Data
{
    public class SOSContext : DbContext
    {
        public SOSContext(DbContextOptions<SOSContext> options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
    }
}
