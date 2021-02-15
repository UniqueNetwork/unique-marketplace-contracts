using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Escrow.Extensions
{
    public static class ServiceScopeFactoryExtensions
    {
        public static async Task MigrateDbAsync(this IServiceScopeFactory scopeFactory, CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
            await dbContext!.Database.MigrateAsync(cancellationToken: cancellationToken);
        }

        public static void MigrateDb(this IServiceScopeFactory scopeFactory)
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
            dbContext!.Database.Migrate();
        }
    }
}