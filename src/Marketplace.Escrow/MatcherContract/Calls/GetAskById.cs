using System.Numerics;
using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;

namespace Marketplace.Escrow.MatcherContract.Calls
{
    public class GetAskById : IContractCallParameter
    {
        [Serialize(0)]
        [U128Converter]
        public BigInteger AskId { get; set; }
    }
}