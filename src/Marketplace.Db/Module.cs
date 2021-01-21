using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Db
{
    public static class Module
    {
        public static void AddDbModule(this IServiceCollection collection, IDbConfiguration configuration)
        {
            collection.AddDbContext<MarketplaceDbContext>(options =>
            {
                options.UseNpgsql(configuration.ConnectionString);
            });
        }
    }
}