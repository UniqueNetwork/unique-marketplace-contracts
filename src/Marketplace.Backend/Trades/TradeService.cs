using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Marketplace.Db;
using Marketplace.Db.Models;
using Microsoft.EntityFrameworkCore;
using Marketplace.Backend.Base58;

namespace Marketplace.Backend.Trades
{
    public class TradeService: ITradeService
    {
        private readonly MarketplaceDbContext _marketplaceDbContext;

        public TradeService(MarketplaceDbContext marketplaceDbContext)
        {
            _marketplaceDbContext = marketplaceDbContext;
        }

        public async Task<PaginationResult<TradeDto>> Get(IReadOnlyCollection<ulong>? collectionIds, PaginationParameter parameter)
        {
            return await 
                FilterByCollectionId(_marketplaceDbContext.Trades, collectionIds)
                .OrderByDescending(t => t.TradeDate)
                .AsNoTrackingWithIdentityResolution()
                .Select(MapTrade())
                .PaginateAsync(parameter);
        }

        private static IQueryable<Trade> FilterByCollectionId(IQueryable<Trade> trades, IReadOnlyCollection<ulong>? collectionIds)
        {
            if (collectionIds?.Any() == true)
            {
                trades = trades.Where(t => collectionIds.Contains(t.Offer.CollectionId));
            }

            return trades;
        }

        private static Expression<Func<Trade, TradeDto>> MapTrade()
        {
            return t => new TradeDto(t.TradeDate, t.Offer.CollectionId, t.Offer.TokenId, t.Offer.Price.ToString(), t.Offer.QuoteId,
                t.Offer.Seller, t.Buyer, t.Offer.Metadata);
        }

        public async Task<IList<TradeDto>> Get(ulong collectionId)
        {
            return await _marketplaceDbContext
                .Trades
                .Where(t => t.Offer.CollectionId == collectionId)
                .OrderByDescending(t => t.TradeDate)
                .AsNoTrackingWithIdentityResolution()
                .Select(MapTrade())
                .ToListAsync();
        }

        public async Task<PaginationResult<TradeDto>> Get(string address, IReadOnlyCollection<ulong>? collectionIds, PaginationParameter parameter)
        {
            // Ensure that address is a proper base58 encoded address
            string base64Address = "Invalid";
            try {
                var pk = AddressEncoding.AddressToPublicKey(address);
                base64Address = Convert.ToBase64String(pk);
            } 
            catch (ArgumentNullException) {} 
            catch (FormatException) {} 
            catch (ArgumentOutOfRangeException) {}
            catch (ArgumentException) {}

            return await 
                FilterByCollectionId(_marketplaceDbContext.Trades, collectionIds)
                .Where(t => (t.Offer.Seller == base64Address) || (t.Buyer == base64Address))
                .OrderByDescending(t => t.TradeDate)
                .AsNoTrackingWithIdentityResolution()
                .Select(MapTrade())
                .PaginateAsync(parameter);
        }
    }
}

