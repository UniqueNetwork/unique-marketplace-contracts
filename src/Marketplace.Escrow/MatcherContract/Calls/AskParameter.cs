using Polkadot.BinaryContracts;
using Polkadot.BinarySerializer;

namespace Marketplace.Escrow.MatcherContract.Calls
{
    public class AskParameter : IContractCallParameter
    {
        [Serialize(0)]
        public ulong CollectionId { get; set; }
        [Serialize(1)]
        public ulong TokenId { get; set; }
        [Serialize(2)]
        public ulong QuoteId { get; set; }
        [Serialize(3)]
        public Balance Price { get; set; } = null!;
    }
}