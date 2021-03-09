using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Marketplace.Db;
using Marketplace.Db.Migrations;
using Marketplace.Db.Models;
using Marketplace.Escrow.EventBus;
using Marketplace.Escrow.MatcherContract.Calls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polkadot.Api;
using Polkadot.BinaryContracts.Calls.Contracts;
using Polkadot.BinaryContracts.Events;
using Polkadot.BinaryContracts.Events.System;
using Polkadot.Data;
using Polkadot.DataStructs;
using Polkadot.Utils;

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
                await Initialize();
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await _eventBusService.ReadRegisterNft(stoppingToken);
                        await Run(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{ServiceName} failed", GetType().FullName);
                    }
                }
            }, stoppingToken);
        }

        private async Task Initialize()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
            var transactions = await dbContext!.NftIncomeTransactions.Where(t => !t.Deposited).ToListAsync();
            var now = DateTime.UtcNow;
            foreach (var transaction in transactions)
            {
                Task.Run(async () =>
                {
                    var lockTime = transaction.LockTime ?? now;
                    if (lockTime < now)
                    {
                        lockTime = now;
                    }

                    await Task.Delay(lockTime - now);
                    await _eventBusService.PublishRegisterNft(CancellationToken.None);
                });
            }
        }

        private async Task Run(CancellationToken stoppingToken)
        {
            while (true)
            {
                NftIncomeTransaction? transaction = null;
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

                CancellationTokenSource refreshLockCancellationTokenSource = new CancellationTokenSource();

                Task.Run(() => RefreshLock(transaction, refreshLockCancellationTokenSource.Token));

                stoppingToken.ThrowIfCancellationRequested();

                SafeApplication application = SafeApplication.CreateApplication(
                    ex => _logger.LogError(ex, "{ServiceName} substrate api failed", GetType().FullName), _logger, _configuration.MatcherContractPublicKey);
                application.Application.Connect(_configuration.UniqueEndpoint);

                var address = new Address(_configuration.MarketplaceUniqueAddress);
                var parameter = new RegisterNftDepositParameter()
                {
                    User = new PublicKey() {Bytes = transaction.OwnerPublicKeyBytes},
                };
                var call = CallCall.Create(0, 200000000000, parameter, application.Application!.Serializer);
                application.HealthCheck(TimeSpan.FromMinutes(10), () =>
                {
                    refreshLockCancellationTokenSource.Cancel();
                    Interlocked.Exchange(ref application, null)?.Dispose();
                });
                var result = await application.Application.SignAndWaitForResult(address, _configuration.MarketplacePrivateKeyBytes, call);
                application.CancelHealthCheck();
                var successful = result.Match(_ => true, fail =>
                {
                    var error = fail.EventArgument0.Value.Match(
                        other => $"Other: {JsonConvert.SerializeObject(other)}",
                        lookup => $"CannotLookup: {JsonConvert.SerializeObject(lookup)}",
                        badOrigin => $"BadOrigin: {JsonConvert.SerializeObject(badOrigin)}",
                        module =>
                        {
                            var meta = application.Application.GetMetadata(null);
                            var moduleError = meta.GetModules().ElementAtOrDefault(module.Index)?.GetErrors()
                                ?.ElementAtOrDefault(module.Error);
                            return $"Module Error: {moduleError?.GetName() ?? ""}, index: {module.Index}, error: {module.Error}";
                        });
                    _logger.LogError("Failed to register NFT via contract, {ErrorText}", error);
                    return false;
                });
                if (!successful)
                {
                    refreshLockCancellationTokenSource.Cancel();
                    Interlocked.Exchange(ref application, null)?.Dispose();
                    continue;
                }
                
                var updated = false;
                do
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
                        var nftIncomeTransaction = dbContext!.NftIncomeTransactions.First(t => t.Id == transaction.Id);
                        
                        nftIncomeTransaction.Deposited = true;
                        await dbContext.SaveChangesAsync();
                        updated = true;
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to dequeue nft item registration");
                        Interlocked.Exchange(ref application, null)?.Dispose();
                        refreshLockCancellationTokenSource.Cancel();
                        throw;
                    }
                } while (!updated);
                
                Interlocked.Exchange(ref application, null)?.Dispose();
                refreshLockCancellationTokenSource.Cancel();
            }
        }
        
        
        private async Task? RefreshLock(NftIncomeTransaction transaction, CancellationToken token)
        {
            await Task.Delay(TimeSpan.FromMinutes(15), token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
            var refresh = await dbContext!.NftIncomeTransactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
            refresh.LockTime = DateTime.UtcNow.AddMinutes(20);
            Task.Run(() => RefreshLock(transaction, token));
        }

    }
}