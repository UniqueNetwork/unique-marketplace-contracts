using Polkadot.BinarySerializer;
using Polkadot.DataStructs;

namespace Marketplace.Escrow.MatcherContract.Calls
{
    public class SetAdminParameter : IContractCallParameter
    {
        [Serialize(0)]
        public PublicKey Admin { get; set; } = null!;
    }
}