using Marketplace.Db;
using Marketplace.Escrow.MatcherContract.Calls;
using Mnemonic;
using Polkadot.DataStructs;
using Polkadot.Utils;

namespace Marketplace.Escrow
{
    public class Configuration: IDbConfiguration
    {
        private string _marketplaceUniqueMnemonic = null!;
        private PublicKey _marketplaceUniquePublicKey = null!;
        private byte[] _marketplacePrivateKeyBytes = null!;
            
        public string ConnectionString { get; set; } = null!;

        public string UniqueEndpoint { get; set; } = null!;

        public string MatcherContractAddress { get; set; } = null!;

        public PublicKey MatcherContractPublicKey => AddressUtils.GetPublicKeyFromAddr(MatcherContractAddress);

        public string MarketplaceUniqueMnemonic
        {
            get => _marketplaceUniqueMnemonic;
            set
            {
                _marketplaceUniqueMnemonic = value;
                value = value.Replace(" ", "\r ");
                value += "\r";
                var pair = MnemonicSubstrate.GeneratePairFromMnemonic(value);
                _marketplacePrivateKeyBytes = pair.Secret.ToBytes();
                _marketplaceUniquePublicKey = new PublicKey() {Bytes = pair.Public.Key};
            }
        }

        public PublicKey MarketplaceUniquePublicKey => _marketplaceUniquePublicKey;

        public string MarketplaceUniqueAddress => AddressUtils.GetAddrFromPublicKey(MarketplaceUniquePublicKey);

        public byte[] MarketplacePrivateKeyBytes => _marketplacePrivateKeyBytes;
    }
}