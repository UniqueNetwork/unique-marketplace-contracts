using System;
using System.Numerics;

namespace Marketplace.Backend.Trades
{
    public record TradeDto(DateTime TradeDate, ulong CollectionId, ulong TokenId, BigInteger Price, string Seller,
        string Buyer, string Metadata);
}