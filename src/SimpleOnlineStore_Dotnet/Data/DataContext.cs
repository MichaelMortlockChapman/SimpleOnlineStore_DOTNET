using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SimpleOnlineStore_Dotnet.Data {
    public class DataContext : IdentityDbContext<IdentityUser> {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    }
}
