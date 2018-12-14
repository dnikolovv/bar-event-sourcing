using Bar.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bar.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Beverage> Beverages { get; set; }
    }
}
