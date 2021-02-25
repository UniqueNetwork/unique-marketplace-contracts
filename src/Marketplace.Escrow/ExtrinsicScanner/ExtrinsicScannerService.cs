using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db;
using Marketplace.Db.Models;
using Marketplace.Escrow.Extensions;
using Marketplace.Escrow.KusamaScanner;
using Marketplace.Escrow.SubstrateScanner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polkadot.Api;
using Polkadot.BinaryContracts.Events;
using Polkadot.BinarySerializer.Extensions;
using Polkadot.Data;
using Polkadot.DataStructs;
using Polkadot.Utils;

namespace Marketplace.Escrow.TransactionScanner
{
    public abstract class ExtrinsicBlockScannerService<TDbBlockModel> : SubstrateBlockScannerService<TDbBlockModel> where TDbBlockModel: class, IProcessedBlock, new()
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        private readonly string _nodeEndpoint;
        private readonly IsolationLevel _isolationLevel;
        protected SafeApplication? _application = null;

        public ExtrinsicBlockScannerService(IServiceScopeFactory scopeFactory, ILogger logger, string nodeEndpoint, IsolationLevel isolationLevel): base(logger, scopeFactory, nodeEndpoint)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _nodeEndpoint = nodeEndpoint;
            _isolationLevel = isolationLevel;
        }

        protected override Task ProcessBlock(ulong blockNumber, CancellationToken stoppingToken)
        {
            TaskCompletionSource<int>? myTask = new ();
            var task = myTask.Task;
            if (_application == null)
            {
                _application = SafeApplication.CreateApplication(ex =>
                {
                    _logger.LogError(ex, "Kusama listener failed");
                    Interlocked.Exchange(ref myTask, null)?.SetException(ex);

                    _application?.Dispose();
                    _application = null;
                }, _logger);
                _application.Application.Connect(_nodeEndpoint);
            }

            Run(blockNumber, stoppingToken).ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    Interlocked.Exchange(ref myTask, null)?.SetCanceled();
                }

                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "Kusama listener failed");
                    Interlocked.Exchange(ref myTask, null)?.SetException(t.Exception!);
                }

                if (t.IsCompletedSuccessfully)
                {
                    Interlocked.Exchange(ref myTask, null)?.SetResult(0);
                }
            });
            
            return task;
        }

        private async Task Run(ulong blockNumber, CancellationToken stoppingToken)
        {
            void OnHealthCheck()
            {
                stoppingToken.ThrowIfCancellationRequested();
                Interlocked.Exchange(ref _application, null)?.Dispose();
                
            }
            
            _application?.HealthCheck(TimeSpan.FromMinutes(1), OnHealthCheck);
            var blockHash = _application!.Application!.GetBlockHash(new GetBlockHashParams() {BlockNumber = blockNumber}).Hash;
            _application.HealthCheck(TimeSpan.FromMinutes(1), OnHealthCheck);
            var block = _application.Application.GetBlock(new GetBlockParams() {BlockHash = blockHash});
            _application.HealthCheck(TimeSpan.FromMinutes(1), OnHealthCheck);
            var eventsString =
                _application.Application.StorageApi.GetStorage("System", "Events", new GetBlockHashParams() {BlockNumber = blockNumber});
            _application.CancelHealthCheck();

            var eventsList = _application.Application.Serializer.Deserialize<EventList>(eventsString.HexToByteArray());
            var extrinsics = block.Block.Extrinsic
                .Select(e => e!.HexToByteArray())
                .Select(e => _application.Application.Serializer.DeserializeAssertReadAll<DeserializedExtrinsic>(e))
                .Where((_, index) => eventsList.ExtrinsicSuccess((uint)index));

            var actions = ProcessExtrinsics(extrinsics, blockNumber, stoppingToken).ToList();

            stoppingToken.ThrowIfCancellationRequested();
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
            await using var transaction = await dbContext!.Database.BeginTransactionAsync(_isolationLevel, stoppingToken);
            try
            {
                var lastProcessedBlock =
                    await dbContext.KusamaProcessedBlocks.FirstOrDefaultAsync(b => b.BlockNumber == blockNumber, stoppingToken);
                if (lastProcessedBlock != null)
                {
                    await transaction.RollbackAsync(stoppingToken);
                    return;
                }
                
                await SaveProcessedBlock(blockNumber, dbContext, stoppingToken);

                foreach (var action in actions)
                {
                    await action(dbContext);
                }

                await dbContext.SaveChangesAsync(stoppingToken);

                await transaction.CommitAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save processing result of {BlockNumber} block", blockNumber);
                // ReSharper disable once MethodSupportsCancellation
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                throw;
            }
        }

        protected abstract IEnumerable<Func<MarketplaceDbContext, Task>> ProcessExtrinsics(IEnumerable<DeserializedExtrinsic> extrinsics, ulong blockNumber, CancellationToken stoppingToken);

        public override void Dispose()
        {
            base.Dispose();
            _application?.Dispose();
        }

    }
}