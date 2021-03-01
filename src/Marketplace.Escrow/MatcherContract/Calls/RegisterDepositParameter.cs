using Polkadot.BinaryContracts;
using Polkadot.BinarySerializer;
using Polkadot.DataStructs;

namespace Marketplace.Escrow.MatcherContract.Calls
{
    public class RegisterDepositParameter : IContractCallParameter
    {
        [Serialize(0)]
        public ulong QuoteId { get; set; }
        [Serialize(1)]
        public Balance DepositBalance { get; set; } = null!;
        [Serialize(2)]
        public PublicKey User { get; set; } = null!;
    }
}