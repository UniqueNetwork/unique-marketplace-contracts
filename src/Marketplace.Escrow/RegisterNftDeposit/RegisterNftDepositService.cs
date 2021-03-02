using System;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db;
using Marketplace.Db.Models;
using Marketplace.Escrow.EventBus;
using Marketplace.Escrow.MatcherContract.Calls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polkadot.BinaryContracts.Calls.Contracts;
using Polkadot.DataStructs;

namespace Marketplace.Escrow.RegisterNftDeposit
{
    public class RegisterNftDepositService : BackgroundService
    {
        private readonly IEventBusService _eventBusService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RegisterNftDepositService> _logger;
        private readonly Configuration _configuration;

        public RegisterNftDepositService(IEventBusService eventBusService, IServiceScopeFactory scopeFactory, ILogger<RegisterNftDepositService> logger, Configuration configuration)
        {
            _eventBusService = eventBusService;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Run(stoppingToken);
                    await _eventBusService.ReadRegisterNft(stoppingToken);
                }
            }, stoppingToken);
        }

        private async Task Run(CancellationToken stoppingToken)
        {
            NftIncomeTransaction transaction = null;
            while (true)
            {
                var now = DateTime.UtcNow;
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
                    transaction = await dbContext!.NftIncomeTransactions.FirstOrDefaultAsync(t =>
                        !t.Deposited && t.LockTime == null || t.LockTime < now, stoppingToken);
                    if (transaction == null)
                    {
                        return;
                    }

                    transaction.LockTime = now.AddMinutes(20);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    continue;
                }

                stoppingToken.ThrowIfCancellationRequested();

                SafeApplication application = SafeApplication.CreateApplication(
                    ex => _logger.LogError(ex, "{ServiceName} substrate api failed", GetType().FullName), _logger);

                var address = new Address(_configuration.MarketplaceUniqueAddress);
                var parameter = new RegisterNftDepositParameter()
                {
                    User = new PublicKey() {Bytes = transaction.OwnerPublicKeyBytes},
                    
                };
                var call = TypedContractCall.Create(0, 0, parameter, application.Application.Serializer);
                application.Application.SignAndSendExtrinsic(address, _configuration.MarketplacePrivateKeyBytes, call,
                   _ =>
                    {
                        var updated = false;
                        do
                        {
                            try
                            {
                                using var scope = _scopeFactory.CreateScope();
                                var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
                                var nftIncomeTransaction = new NftIncomeTransaction()
                                {
                                    Id = transaction.Id
                                };
                                dbContext.Attach(nftIncomeTransaction);
                                nftIncomeTransaction.Deposited = true;
                                dbContext.SaveChanges();
                                updated = true;
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to dequeue nft item registration");
                                throw;
                            }
                        } while (!updated);
                        
                        Interlocked.Exchange(ref application, null)?.Dispose();
                    });
            }
        }
    }
}