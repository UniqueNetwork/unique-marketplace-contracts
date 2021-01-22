using System;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marketplace.Escrow
{
    public class DaemonService: BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DaemonService> _logger;

        public DaemonService(IServiceScopeFactory scopeFactory, ILogger<DaemonService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
                await dbContext!.Database.MigrateAsync(cancellationToken: stoppingToken);
            }

            int i = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{i++}");
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}