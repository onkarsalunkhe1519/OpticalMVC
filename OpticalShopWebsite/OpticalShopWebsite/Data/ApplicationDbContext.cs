using Microsoft.EntityFrameworkCore;
using OpticalShopWebsite.Models;

namespace OpticalShopWebsite.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<EyeReport> EyeReports { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Salary> Salaries { get; set; }


    }
}
