using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MAPI.Models
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        { 

        }
        public DbSet<Material> Materials {get; set;}
        public DbSet<B_Bill> Bills { get; set;}
        public DbSet<B_BillItem> B_BillItem {get; set;}
        public DbSet<S_Bill> S_Bills { get; set; }

        public DbSet<S_BillItem> S_BillItems { get; set; }
        public DbSet<Stock> Stocks { get; set; }

    }
}
