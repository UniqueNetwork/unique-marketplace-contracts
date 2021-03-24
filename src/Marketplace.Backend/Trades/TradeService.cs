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

        public async Task<IList<TradeDto>> Get()
        {
            return await _marketplaceDbContext
                .Trades
                .OrderByDescending(t => t.TradeDate)
                .AsNoTrackingWithIdentityResolution()
                .Select(MapTrade())
                .ToListAsync();

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

        public async Task<IList<TradeDto>> Get(string seller)
        {
            // Ensure that seller is a proper base58 encoded address
            string base64Seller = "Invalid";
            try {
                var pk = AddressEncoding.AddressToPublicKey(seller);
                base64Seller = Convert.ToBase64String(pk);
            } 
            catch (ArgumentNullException) {} 
            catch (FormatException) {} 
            catch (ArgumentOutOfRangeException) {}
            catch (ArgumentException) {}

            return await _marketplaceDbContext
                .Trades
                .Where(t => t.Offer.Seller == base64Seller)
                .OrderByDescending(t => t.TradeDate)
                .AsNoTrackingWithIdentityResolution()
                .Select(MapTrade())
                .ToListAsync();

        }
    }
}

