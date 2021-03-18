using System.Numerics;

namespace Marketplace.Backend.Offers
{
    public record OfferDto(ulong CollectionId, ulong TokenId, string Price, ulong QuoteId, string Seller, string Metadata);
}