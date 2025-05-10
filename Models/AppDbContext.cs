//using MAPI.Migrations;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MAPI.Models
{
    public class AppDbContext : IdentityDbContext, IDataProtectionKeyContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Material> Materials { get; set; }
        public DbSet<B_Bill> B_Bill { get; set; }
        public DbSet<B_BillItem> B_BillItem { get; set; }
        public DbSet<C_Bill> C_Bills { get; set; }
        
        public DbSet<Stock> Stocks { get; set; }

        public DbSet<User> user { get; set; }

        public DbSet<Party> Party { get; set; }

       public DbSet<States> states { get; set; }
         


        // ✅ Ensure correct property name, type, and namespace
        // Implementing IDataProtectionKeyContext
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}
