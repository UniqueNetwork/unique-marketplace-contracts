using Polkadot.BinarySerializer;

namespace Marketplace.Escrow.MatcherContract.Calls
{
    public class GetAskIdByTokenParameter : IContractCallParameter
    {
        [Serialize(0)]
        public ulong CollectionId { get; set; }
        [Serialize(1)]
        public ulong TokenId { get; set; }
    }
}