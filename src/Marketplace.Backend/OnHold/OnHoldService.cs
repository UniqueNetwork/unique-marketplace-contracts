using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Marketplace.Backend.Base58;
using Marketplace.Db;
using Marketplace.Db.Models;
using Microsoft.EntityFrameworkCore;

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
            return _marketplaceDbContext.NftIncomingTransactions
                .Where(n => n.OfferId == null);
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
            
            var incomings = IncomingWithoutOffer()
                .Where(i => i.OwnerPublicKey == base64Owner);
            if (collectionIds?.Any() == true)
            {
                incomings = incomings.Where(i => collectionIds.Contains(i.CollectionId));
            }

            return incomings.Select(MapOnHold()).PaginateAsync(parameter);
        }

        public async Task ConnectOffersAndNftIncomes()
        {
            var incomes = await _marketplaceDbContext.NftIncomingTransactions
                .Where(income => income.OfferId == null)
                .OrderBy(income => income.CollectionId)
                .ThenBy(income => income.TokenId)
                .ThenBy(income => income.UniqueProcessedBlockId)
                .ToListAsync();

            var offers = await (
                    from offer in _marketplaceDbContext.Offers
                    from income in _marketplaceDbContext.NftIncomingTransactions.Where(i => i.OfferId == offer.Id)
                        .DefaultIfEmpty()
                    where income == null
                    select offer
                )
                .OrderBy(offer => offer.CollectionId)
                .ThenBy(offer=> offer.TokenId)
                .ThenBy(offer => offer.CreationDate)
                .ToListAsync();

            var incomeIndex = 0;
            var offerIndex = 0;
            while (incomeIndex < incomes.Count && offerIndex < offers.Count)
            {
                var income = incomes[incomeIndex];
                var offer = offers[offerIndex];
                if (income.CollectionId == offer.CollectionId && income.TokenId == offer.TokenId)
                {
                    incomeIndex++;
                    offerIndex++;
                    income.Offer = offer;
                    await _marketplaceDbContext.SaveChangesAsync();
                }

                if (income.CollectionId == offer.CollectionId)
                {
                    if (income.TokenId > offer.TokenId)
                    {
                        offerIndex++;
                    }
                    else
                    {
                        incomeIndex++;
                    }
                }
                else
                {
                    if (income.CollectionId > offer.CollectionId)
                    {
                        offerIndex++;
                    }
                    else
                    {
                        incomeIndex++;
                    }
                }
            }
        }
    }
}