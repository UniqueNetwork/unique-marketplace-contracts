using System;
using System.Numerics;

namespace Marketplace.Backend.Trades
{
    public record TradeDto(DateTime TradeDate, ulong CollectionId, ulong TokenId, string Price, ulong QuoteId, string Seller,
        string Buyer, string Metadata);
}