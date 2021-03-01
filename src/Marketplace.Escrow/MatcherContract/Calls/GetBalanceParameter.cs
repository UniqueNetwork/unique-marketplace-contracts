using Polkadot.BinarySerializer;

namespace Marketplace.Escrow.MatcherContract.Calls
{
    public class GetBalanceParameter : IContractCallParameter
    {
        [Serialize(0)]
        public ulong QuoteId { get; set; }
    }
}