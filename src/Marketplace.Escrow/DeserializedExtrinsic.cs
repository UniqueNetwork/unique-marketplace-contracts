using Polkadot.BinaryContracts;
using Polkadot.BinaryContracts.Calls;
using Polkadot.BinaryContracts.Extrinsic;
using Polkadot.BinarySerializer;

namespace Marketplace.Escrow
{
    public class DeserializedExtrinsic
    {
        [Serialize(0)]
        public UncheckedExtrinsic<ExtrinsicAddress, ExtrinsicMultiSignature, SignedExtra, InheritanceCall<IExtrinsicCall>> Extrinsic
        {
            get;
            set;
        } = null!;
    }
}