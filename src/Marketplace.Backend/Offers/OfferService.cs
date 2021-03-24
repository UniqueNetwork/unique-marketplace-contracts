using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Marketplace.Db;
using Marketplace.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Backend.Offers
{
    public class OfferService: IOfferService
    {
        private readonly MarketplaceDbContext _marketplaceDbContext;
        private readonly Configuration _configuration;

        public OfferService(MarketplaceDbContext marketplaceDbContext, Configuration configuration)
        {
            _marketplaceDbContext = marketplaceDbContext;
            _configuration = configuration;
        }


        public async Task<IList<OfferDto>> Get()
        {
            return await _marketplaceDbContext
                .Offers
                .OrderByDescending(o => o.CreationDate)
                .Take(_configuration.DefaultRequestLimit)
                .AsNoTrackingWithIdentityResolution()
                .Select(MapOfferDto())
                .ToListAsync();
        }

        private static Expression<Func<Offer, OfferDto>> MapOfferDto()
        {
            return o => new OfferDto(o.CollectionId, o.TokenId, o.Price.ToString(), o.QuoteId, o.Seller, o.Metadata);
        }

        public async Task<IList<OfferDto>> Get(ulong collectionId)
        {
            return await _marketplaceDbContext
                .Offers
                .Where(o => o.CollectionId == collectionId)
                .OrderByDescending(o => o.CreationDate)
                .AsNoTrackingWithIdentityResolution()
                .Select(MapOfferDto())
                .ToListAsync();
        }

        public async Task<IList<OfferDto>> Get(string seller)
        {
            // Ensure that seller is a proper base64 encoded public key
            try {
                byte[] data = Convert.FromBase64String(seller);
                if (data.Length != 32) seller = "invalid";
            } catch (ArgumentNullException) {
                seller = "invalid";
            } catch (FormatException) {
                seller = "invalid";
            }

            return await _marketplaceDbContext
                .Offers
                .Where(o => o.Seller == seller)
                .OrderByDescending(o => o.CreationDate)
                .AsNoTrackingWithIdentityResolution()
                .Select(MapOfferDto())
                .ToListAsync();
        }
    }
}