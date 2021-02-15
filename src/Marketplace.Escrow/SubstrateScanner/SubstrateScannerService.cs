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
    public abstract class SubstrateScannerService<TDbBlockModel>: BackgroundService where TDbBlockModel: class, IProcessedBlock, new()
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _nodeEndpoint;
        private readonly TaskCompletionSource<int> _executionCompletionSource = new();
        private readonly Channel<ulong> _blocksChannel = Channel.CreateUnbounded<ulong>();
        private ulong _lastKusamaBlock = 0;
        private ulong _lastProcessedBlock = 0;

        protected SubstrateScannerService(ILogger logger, IServiceScopeFactory scopeFactory, string nodeEndpoint)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _nodeEndpoint = nodeEndpoint;
        }

        public Application CreateApplication(Action<Exception> onError)
        {
            var param = new JsonRpcParams {JsonrpcVersion = "2.0"};

            var logger = new SubstrateApiLogger(_logger);
            var jsonRpc = new JsonRpc(new Wsclient(logger), logger, param, onError);

            return new Application(logger, jsonRpc, Application.DefaultSubstrateSettings());
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _scopeFactory.MigrateDb();
            _logger.LogInformation("Started {ServiceName}", GetType().FullName);
            InitBlocksSubscription(stoppingToken);

            return _executionCompletionSource.Task;
        }

        private async Task InitBlocksSubscription(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetService<MarketplaceDbContext>();
            var set = context!.Set<TDbBlockModel>();
            _lastKusamaBlock = await set.MaxAsync(s => (ulong?) s.BlockNumber, stoppingToken) + 1 ?? 1;
            _lastProcessedBlock = _lastKusamaBlock;
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

            await foreach (var blockNumber in _blocksChannel.Reader.ReadAllAsync(stoppingToken))
            {
                var successfullyCompleted = false;
                do
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    try
                    {
                        _logger.LogInformation("Processing block # {BlockNumber}", _lastProcessedBlock);
                        stoppingToken.ThrowIfCancellationRequested();
                        await ProcessBlock(_lastProcessedBlock, stoppingToken);
                        _logger.LogInformation("Finished processing block # {BlockNumber}", _lastProcessedBlock);
                        _lastProcessedBlock++;
                        successfullyCompleted = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process block # ${BlockNumber}", _lastProcessedBlock);
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }

                } while (successfullyCompleted);
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
            Application? application = null;
            application = CreateApplication(ex =>
            {
                if (application == null)
                {
                    return;
                }
                
                _logger.LogError(ex, "Application failed in service {ServiceName}, reconnecting", GetType().FullName);
                try
                {
                    application?.Dispose();
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                }
                finally
                {
                    application = null;
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    // ReSharper disable once MethodSupportsCancellation
                    Task.Delay(TimeSpan.FromSeconds(30), stoppingToken)
                        .ContinueWith(_ => SubscribeBlocks(stoppingToken), stoppingToken);
                }
            });
            application.Connect(_nodeEndpoint);

            application.SubscribeBlockNumber(block =>
            {
                stoppingToken.ThrowIfCancellationRequested();
                for (ulong i = _lastKusamaBlock; i <= (ulong)block; i++)
                {
                    _logger.LogInformation("Scheduled block # {BlockNumber}", _lastKusamaBlock);
                    _lastKusamaBlock++;
                    _blocksChannel.Writer.WriteAsync(i, stoppingToken).GetAwaiter().GetResult();
                }
                
                if (stoppingToken.IsCancellationRequested)
                {
                    application.Dispose();
                }
            });
        }

        protected abstract Task ProcessBlock(ulong blockNumber, CancellationToken stoppingToken);
    }
}