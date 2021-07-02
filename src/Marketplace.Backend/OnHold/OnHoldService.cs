using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Marketplace.Backend.Base58;
using Marketplace.Db;
using Marketplace.Db.Models;

namespace Marketplace.Backend.OnHold
{
    public class OnHoldService : IOnHoldService
    {
        private readonly MarketplaceDbContext _marketplaceDbContext;

        public OnHoldService(MarketplaceDbContext marketplaceDbContext)
        {
            _marketplaceDbContext = marketplaceDbContext;
        }
        
        public Task<PaginationResult<OnHold>> Get(IReadOnlyCollection<ulong>? collectionIds, PaginationParameter parameter)
        {
            var incomings = IncomingWithoutOffer();
            if (collectionIds?.Any() == true)
            {
                incomings = incomings.Where(i => collectionIds.Contains(i.CollectionId));
            }

            return incomings.Select(MapOnHold()).PaginateAsync(parameter);
        }

        private Expression<Func<NftIncomingTransaction, OnHold>> MapOnHold()
        {
            return income => new OnHold(income.CollectionId, income.TokenId, income.OwnerPublicKey);
        }

        private IQueryable<NftIncomingTransaction> IncomingWithoutOffer()
        {
            return from income in _marketplaceDbContext.NftIncomingTransactions
                from offer in _marketplaceDbContext.Offers.Where(o =>
                    o.CollectionId == income.CollectionId && o.TokenId == income.TokenId &&
                    o.OfferStatus == OfferStatus.Active).DefaultIfEmpty()
                where offer == null
                select income;
        }

        public Task<PaginationResult<OnHold>> Get(string owner, IReadOnlyCollection<ulong>? collectionIds, PaginationParameter parameter)
        {
            // Ensure that seller is a proper base58 encoded address
            string base64Owner = "Invalid";
            try {
                var pk = AddressEncoding.AddressToPublicKey(owner);
                base64Owner = Convert.ToBase64String(pk);
            } 
            catch (ArgumentNullException) {} 
            catch (FormatException) {} 
            catch (ArgumentOutOfRangeException) {}
            catch (ArgumentException) {}
            // Console.WriteLine($"Converted {seller} to base64: {base64Seller}");
            
            var incomings = IncomingWithoutOffer().Where(i => i.OwnerPublicKey == base64Owner);
            if (collectionIds?.Any() == true)
            {
                incomings = incomings.Where(i => collectionIds.Contains(i.CollectionId));
            }

            return incomings.Select(MapOnHold()).PaginateAsync(parameter);
        }
    }
}