using CurrencyExchanger.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchanger.Infrastructure
{
    public class AppDbContext : DbContext
    {
        // DbSet for CurrencyRates
        public DbSet<CurrencyRate> CurrencyRates { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        // Constructor
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Configure the model (optional but recommended)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define the schema for CurrencyRate
            modelBuilder.Entity<CurrencyRate>(entity =>
            {
                entity.HasKey(e => new { e.CurrencyCode, e.Date }); // Composite key
                entity.Property(e => e.CurrencyCode)
                    .HasMaxLength(3)
                    .IsRequired(); // ISO code
                entity.Property(e => e.Rate)
                    .HasColumnType("decimal(18,4)") // Define SQL type to avoid truncation
                    .IsRequired();
                entity.Property(e => e.Date)
                    .IsRequired(); // Date of rate
            });
            
            
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(e => e.Id); // Primary key
                entity.Property(e => e.Currency)
                    .HasMaxLength(3)
                    .IsRequired(); // ISO currency code
                entity.Property(e => e.Balance)
                    .HasColumnType("decimal(18,4)") // Define SQL type for accuracy
                    .IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}