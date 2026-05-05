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

        public DbSet<Payment> Payments { get; set; }
        public DbSet<FraudAttempt> FraudAttempts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.Reference)
                .IsUnique();
        }
    }
}