using System.Globalization;
using System.Numerics;
using Marketplace.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.TypeHandlers.NumericHandlers;

namespace Marketplace.Db
{
    public class MarketplaceDbContext : DbContext
    {
        public DbSet<Offer> Offers { get; set; } = null!;
        public DbSet<Trade> Trades { get; set; } = null!;
        public DbSet<KusamaProcessedBlock> KusamaProcessedBlocks { get; set; } = null!;
        public DbSet<KusamaTransaction> KusamaTransactions { get; set; } = null!;
        public DbSet<UniqueProcessedBlock> UniqueProcessedBlocks { get; set; } = null!;
        public DbSet<NftIncomeTransaction> NftIncomeTransactions { get; set; } = null!;

        public MarketplaceDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var bigIntegerConverter = new ValueConverter<BigInteger, string>(
                model => model.ToString(CultureInfo.InvariantCulture),
                provider => BigInteger.Parse(provider));

            modelBuilder.Entity<Offer>()
                .Property(e => e.Price)
                .HasConversion(bigIntegerConverter);

            modelBuilder.Entity<KusamaTransaction>()
                .Property(e => e.Amount)
                .HasConversion(bigIntegerConverter);

            modelBuilder.Entity<NftIncomeTransaction>()
                .HasIndex(e => e.Deposited)
                .HasFilter($"\"Deposited\" is not true");
                
            modelBuilder.Entity<NftIncomeTransaction>()
                .Property(e => e.Value)
                .HasConversion(bigIntegerConverter);
        }
    }
}