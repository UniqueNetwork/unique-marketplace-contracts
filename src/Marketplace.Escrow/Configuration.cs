using Marketplace.Db;

namespace Marketplace.Escrow
{
    public class Configuration: IDbConfiguration
    {
        public string ConnectionString { get; set; } = null!;
    }
}