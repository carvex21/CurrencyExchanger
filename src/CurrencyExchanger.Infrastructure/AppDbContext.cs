using CurrencyExchanger.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchanger.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<Wallet> Wallets { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurrencyRate>(entity =>
            {
                entity.HasKey(e => new { e.CurrencyCode, e.Date });
                entity.Property(e => e.CurrencyCode)
                    .HasMaxLength(3)
                    .IsRequired();
                entity.Property(e => e.Rate)
                    .HasColumnType("decimal(18,4)")
                    .IsRequired();
                entity.Property(e => e.Date)
                    .IsRequired();
            });

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Currency)
                    .HasMaxLength(3)
                    .IsRequired();
                entity.Property(e => e.Balance)
                    .HasColumnType("decimal(18,4)")
                    .IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}