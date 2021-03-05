using Polkadot.BinarySerializer;
using Polkadot.DataStructs;

namespace Marketplace.Escrow.MatcherContract.Calls
{
    public class WithdrawParameter : IContractCallParameter
    {
        [Serialize(0)]
        public ulong QuoteId { get; set; }
        [Serialize(1)]
        public PublicKey WithdrawBalance { get; set; } = null!;
    }
}