using System.Numerics;

namespace Marketplace.Backend.Offers
{
    public record OfferDto(ulong CollectionId, ulong TokenId, BigInteger Price, string Seller, string Metadata);
}