using System;
using System.Threading.Tasks;
using Marketplace.Db;

namespace Marketplace.Escrow.TransactionScanner
{
    public class ExtrinsicHandler
    {
        public Func<MarketplaceDbContext, ValueTask>? OnSaveToDb { get; set; }
        public Func<ValueTask>? OnAfterSaveToDb { get; set; }
    }
}