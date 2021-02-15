using Marketplace.Db;
using Polkadot.DataStructs;
using Polkadot.Utils;

namespace Marketplace.Escrow
{
    public class Configuration: IDbConfiguration
    {
        public string ConnectionString { get; set; } = null!;
        
        public string KusamaEndpoint { get; set; } = null!;

        public string MarketplaceKusamaAddress { get; set; } = null!;

        public PublicKey MarketplaceKusamaPublicKey => AddressUtils.GetPublicKeyFromAddr(MarketplaceKusamaAddress);
    }
}