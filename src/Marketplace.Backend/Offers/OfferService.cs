using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Marketplace.Db;
using Marketplace.Db.Models;
using Microsoft.EntityFrameworkCore;
using Marketplace.Backend.Base58;

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

        public async Task<PaginationResult<OfferDto>> Get(ulong? collectionId, PaginationParameter parameter)
        {
            return await 
                FilterByCollectionId(_marketplaceDbContext.Offers, collectionId)
                .Where(o => o.OfferStatus == OfferStatus.Active)
                .OrderByDescending(o => o.CreationDate)
                .AsNoTrackingWithIdentityResolution()
                .Select(MapOfferDto())
                .PaginateAsync(parameter);
        }
        
        private static IQueryable<Offer> FilterByCollectionId(IQueryable<Offer> offers, ulong? collectionId)
        {
            if (collectionId.HasValue)
            {
                offers = offers.Where(o => o.CollectionId == collectionId);
            }

            return offers;
        }

        private static Expression<Func<Offer, OfferDto>> MapOfferDto()
        {
            return o => new OfferDto(o.CollectionId, o.TokenId, o.Price.ToString(), o.QuoteId, o.Seller, o.Metadata);
        }

        public async Task<IList<OfferDto>> Get(ulong collectionId)
        {
            return await _marketplaceDbContext
                .Offers
                .Where(o => o.CollectionId == collectionId && o.OfferStatus == OfferStatus.Active)
                .OrderByDescending(o => o.CreationDate)
                .AsNoTrackingWithIdentityResolution()
                .Select(MapOfferDto())
                .ToListAsync();
        }

        public async Task<PaginationResult<OfferDto>> Get(string seller, ulong? collectionId, PaginationParameter paginationParameter)
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
            // Console.WriteLine($"Converted {seller} to base64: {base64Seller}");

            return await 
                FilterByCollectionId(_marketplaceDbContext.Offers, collectionId)
                .Where(o => o.Seller == base64Seller && o.OfferStatus == OfferStatus.Active)
                .OrderByDescending(o => o.CreationDate)
                .AsNoTrackingWithIdentityResolution()
                .Select(MapOfferDto())
                .PaginateAsync(paginationParameter);
        }
    }
}