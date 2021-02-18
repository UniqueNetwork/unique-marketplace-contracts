using Polkadot.BinaryContracts;
using Polkadot.BinaryContracts.Calls;
using Polkadot.BinaryContracts.Extrinsic;
using Polkadot.BinarySerializer;

namespace Marketplace.Escrow
{
    public class DeserializedExtrinsic
    {
        [Serialize(0)]
        public AsByteVec<UncheckedExtrinsic<ExtrinsicAddress, ExtrinsicMultiSignature, SignedExtra, InheritanceCall<IExtrinsicCall>>> Vec
        {
            get;
            set;
        } = null!;

        public UncheckedExtrinsic<ExtrinsicAddress, ExtrinsicMultiSignature, SignedExtra, InheritanceCall<IExtrinsicCall>> Extrinsic => Vec.Value;
    }
}