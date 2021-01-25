using Marketplace.Db;

namespace Marketplace.Backend
{
    public class Configuration : IDbConfiguration
    {
        public string ConnectionString { get; set; } = null!;
        public int DefaultRequestLimit { get; set; }
    }
}