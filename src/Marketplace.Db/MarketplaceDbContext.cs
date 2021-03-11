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
        public DbSet<QuoteIncomingTransaction> QuoteIncomingTransactions { get; set; } = null!;
        public DbSet<UniqueProcessedBlock> UniqueProcessedBlocks { get; set; } = null!;
        public DbSet<NftIncomingTransaction> NftIncomingTransactions { get; set; } = null!;
        public DbSet<NftOutgoingTransaction> NftOutgoingTransactions { get; set; } = null!;
        public DbSet<QuoteOutgoingTransaction> QuoteOutgoingTransactions { get; set; } = null!;

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

            modelBuilder.Entity<NftIncomingTransaction>()
                .HasIndex("Status", "LockTime")
                .HasFilter($"\"Status\" = 0");

            modelBuilder.Entity<NftIncomingTransaction>()
                .Property(e => e.Value)
                .HasConversion(bigIntegerConverter);

            modelBuilder.Entity<QuoteIncomingTransaction>()
                .HasIndex("Status", "LockTime")
                .HasFilter($"\"Status\" = 0");

            modelBuilder.Entity<QuoteIncomingTransaction>()
                .Property(e => e.Amount)
                .HasConversion(bigIntegerConverter);

            modelBuilder.Entity<QuoteOutgoingTransaction>()
                .HasIndex("Status")
                .HasFilter($"\"Status\" = 0");

            modelBuilder.Entity<QuoteOutgoingTransaction>()
                .Property(e => e.Value)
                .HasConversion(bigIntegerConverter);

            modelBuilder.Entity<NftOutgoingTransaction>()
                .Property(e => e.Value)
                .HasConversion(bigIntegerConverter);

            modelBuilder.Entity<NftOutgoingTransaction>()
                .HasIndex("Status", "LockTime")
                .HasFilter($"\"Status\" = 0");
        }
    }
}