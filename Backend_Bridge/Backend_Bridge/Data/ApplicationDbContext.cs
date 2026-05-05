using Microsoft.EntityFrameworkCore;
using Backend_Bridge.Models;

namespace Backend_Bridge.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SmsLog> SmsLogs { get; set; }
        public DbSet<Order> Orders { get; set; }
    }
}