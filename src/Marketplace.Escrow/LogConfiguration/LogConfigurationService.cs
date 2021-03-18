using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using Polkadot.Api;

namespace Marketplace.Escrow.LogConfiguration
{
    public class LogConfigurationService : BackgroundService
    {
        private readonly Configuration _configuration;
        private readonly ILogger<LogConfigurationService> _logger;

        public LogConfigurationService(Configuration configuration, ILogger<LogConfigurationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var completionSource = new TaskCompletionSource();
            var task = completionSource.Task;
            _logger.LogInformation("Configuration: UniqueEndpoint {UniqueEndpoint}, MatcherContractAddress {MatcherContractAddress}, MarketplaceUniqueAddress {MarketplaceUniqueAddress}", 
                _configuration.UniqueEndpoint, 
                _configuration.MatcherContractAddress, 
                _configuration.MarketplaceUniqueAddress);

            SafeApplication? application = null;
            application = SafeApplication.CreateApplication(ex =>
            {
                _logger.LogError(ex, "{ServiceName} failed", GetType().FullName);
                Interlocked.Exchange<TaskCompletionSource?>(ref completionSource, null)?.SetException(ex);
                application?.Dispose();
            }, _logger, _configuration.MatcherContractPublicKey);
            application.Application.Connect(_configuration.UniqueEndpoint);
            application.Application!.SubscribeAccountInfo(_configuration.MarketplaceUniqueAddress, info =>
            {
                Interlocked.Exchange<TaskCompletionSource?>(ref completionSource, null)?.SetResult();
                application.Dispose();
                _logger.LogInformation("Admins balance: Free: {Free}, Reserved: {Reserved}, FeeFrozen: {FeeFrozen}, MiscFrozen: {MiscFrozen}", 
                    info.AccountData.Free, 
                    info.AccountData.Reserved,
                    info.AccountData.FeeFrozen,
                    info.AccountData.MiscFrozen);
            });

            return task;
        }
    }
}