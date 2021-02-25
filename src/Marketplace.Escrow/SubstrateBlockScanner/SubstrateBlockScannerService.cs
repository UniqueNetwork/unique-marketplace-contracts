using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Marketplace.Db;
using Marketplace.Db.Models;
using Marketplace.Escrow.ApiLogger;
using Marketplace.Escrow.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polkadot.Api;

namespace Marketplace.Escrow.SubstrateScanner
{
    public abstract class SubstrateBlockScannerService<TDbBlockModel>: BackgroundService where TDbBlockModel: class, IProcessedBlock, new()
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _nodeEndpoint;
        private readonly TaskCompletionSource<int> _executionCompletionSource = new();
        private readonly Channel<ulong> _blocksChannel = Channel.CreateUnbounded<ulong>();
        private ulong _lastScheduledBlock = 0;
        private ulong _lastProcessedBlock = 0;
        private object _blockSchedulerLock = new object();

        protected SubstrateBlockScannerService(ILogger logger, IServiceScopeFactory scopeFactory, string nodeEndpoint)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _nodeEndpoint = nodeEndpoint;
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var migrated = false;
            do
            {
                try
                {
                    _scopeFactory.MigrateDb();
                    migrated = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to migrate database to latest version");
                    Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).GetAwaiter().GetResult();
                }
            } while (!migrated);
            _logger.LogInformation("Started {ServiceName}", GetType().FullName);
#pragma warning disable 4014
            InitBlocksSubscription(stoppingToken);
#pragma warning restore 4014

            return _executionCompletionSource.Task;
        }

        private async Task InitBlocksSubscription(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetService<MarketplaceDbContext>();
            var set = context!.Set<TDbBlockModel>();
            _lastProcessedBlock = await set.MaxAsync(s => (ulong?) s.BlockNumber, stoppingToken) ?? 0;
            _lastScheduledBlock = _lastProcessedBlock;
            SubscribeBlocks(stoppingToken);
            StartProcessing(stoppingToken);
        }

        private void StartProcessing(CancellationToken stoppingToken)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Process(stoppingToken);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogInformation(ex, "Received stop signal, shutting down");
                    // ReSharper disable once MethodSupportsCancellation
                    _executionCompletionSource.SetCanceled();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Service {ServiceName} failed, restarting", GetType().FullName);
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    StartProcessing(stoppingToken);
                }
            }, stoppingToken);
        }

        private async Task Process(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            while(!stoppingToken.IsCancellationRequested)
            {
                var blockNumber = await _blocksChannel.Reader.ReadAsync(stoppingToken);
                var successfullyCompleted = false;
                do
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    try
                    {
                        _logger.LogInformation("Processing block # {BlockNumber}", _lastProcessedBlock + 1);
                        stoppingToken.ThrowIfCancellationRequested();
                        await ProcessBlock(_lastProcessedBlock + 1, stoppingToken);
                        _logger.LogInformation("Finished processing block # {BlockNumber}", _lastProcessedBlock + 1);
                        _lastProcessedBlock++;
                        successfullyCompleted = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process block # ${BlockNumber}", _lastProcessedBlock + 1);
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }

                } while (!successfullyCompleted);
            }
        }

        protected async Task SaveProcessedBlock(ulong blockNumber, MarketplaceDbContext dbContext, CancellationToken stoppingToken)
        {
            var processedBlock = new TDbBlockModel
            {
                BlockNumber = blockNumber,
                ProcessDate = DateTime.UtcNow
            };
            await dbContext.Set<TDbBlockModel>().AddAsync(processedBlock, stoppingToken);
            await dbContext.SaveChangesAsync(stoppingToken);
        }

        private void SubscribeBlocks(CancellationToken stoppingToken)
        {
            void Resubscribe()
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    // ReSharper disable once MethodSupportsCancellation
                    Task.Delay(TimeSpan.FromSeconds(30), stoppingToken)
                        .ContinueWith(_ => SubscribeBlocks(stoppingToken), stoppingToken);
                }
            }

            SafeApplication? application = null;
            application = SafeApplication.CreateApplication(ex =>
            {
                _logger.LogError(ex, "Application failed in service {ServiceName}, reconnecting", GetType().FullName);
                try
                {
                    application?.Dispose();
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                }

                Resubscribe();
            }, _logger);
            application?.Application?.Connect(_nodeEndpoint);

            
            Action healthCheck = () =>
            {
                application?.HealthCheck(TimeSpan.FromMinutes(2), () =>
                {
                    application?.Dispose();
                    if (stoppingToken.IsCancellationRequested)
                    {
                        Resubscribe();
                    }
                });
            };
            
            application?.Application?.SubscribeBlockNumber(block =>
            {
                application?.CancelHealthCheck();
                lock (_blockSchedulerLock)
                {
                    while (_lastScheduledBlock + 1 <= (ulong)block)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            application?.Dispose();
                            return;
                        }

                        _logger.LogInformation("Scheduled block # {BlockNumber}", _lastScheduledBlock + 1);
                        _blocksChannel.Writer.WriteAsync(_lastScheduledBlock + 1, stoppingToken).GetAwaiter().GetResult();
                        _lastScheduledBlock++;
                    }
                }

                healthCheck();
            });
            healthCheck();
        }

        protected abstract Task ProcessBlock(ulong blockNumber, CancellationToken stoppingToken);
    }
}