using Bar.Data;
using Bar.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Bar.Web.Configuration
{
    public static class DbContextExtensions
    {
        public static void Seed(this ApplicationDbContext dbContext)
        {
            dbContext.Database.Migrate();

            if (!dbContext.Beverages.Any())
            {
                var beverages = new List<Beverage>
                {
                    new Beverage { Description = "Whisky", MenuNumber = 1, Price = 4.00m },
                    new Beverage { Description = "Coke",   MenuNumber = 2, Price = 1.00m },
                    new Beverage { Description = "Vodka",  MenuNumber = 3, Price = 4.00m },
                    new Beverage { Description = "Juice",  MenuNumber = 4, Price = 1.00m },
                };

                dbContext.Beverages.AddRange(beverages);
                dbContext.SaveChanges();
            }
        }
    }
}
