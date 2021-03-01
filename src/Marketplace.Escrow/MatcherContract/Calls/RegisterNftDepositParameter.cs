using Polkadot.BinarySerializer;
using Polkadot.DataStructs;

namespace Marketplace.Escrow.MatcherContract.Calls
{
    public class RegisterNftDepositParameter : IContractCallParameter
    {
        [Serialize(0)]
        public ulong CollectionId { get; set; }
        [Serialize(1)]
        public ulong TokenId { get; set; }
        [Serialize(2)]
        public PublicKey User { get; set; } = null!;
    }
}