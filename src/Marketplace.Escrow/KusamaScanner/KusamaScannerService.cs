using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db;
using Marketplace.Db.Models;
using Marketplace.Escrow.ApiLogger;
using Marketplace.Escrow.Extensions;
using Marketplace.Escrow.SubstrateScanner;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polkadot.Api;
using Polkadot.BinaryContracts.Events;
using Polkadot.BinarySerializer.Extensions;
using Polkadot.Data;
using Polkadot.DataStructs;
using Polkadot.Utils;
using StrobeNet.Extensions;

namespace Marketplace.Escrow.KusamaScanner
{
    public class KusamaScannerService: SubstrateScannerService<KusamaProcessedBlock>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<KusamaScannerService> _logger;
        private readonly Configuration _configuration;
        private SafeApplication? _application = null;
        private PublicKey _marketplacePublicKey;

        public KusamaScannerService(IServiceScopeFactory scopeFactory, ILogger<KusamaScannerService> logger, Configuration configuration): base(logger, scopeFactory, configuration.KusamaEndpoint)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
            _marketplacePublicKey = configuration.MarketplaceKusamaPublicKey;
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
                _application.Application.Connect(_configuration.KusamaEndpoint);
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
                .Select(e => _application.Application.Serializer.DeserializeAssertReadAll<DeserializedExtrinsic>(e));

            var actions = ProcessExtrinsics(extrinsics, eventsList, blockNumber, stoppingToken).ToList();

            stoppingToken.ThrowIfCancellationRequested();
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
            await using var transaction = await dbContext!.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, stoppingToken);
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

        private IEnumerable<Func<MarketplaceDbContext, Task>> ProcessExtrinsics(IEnumerable<DeserializedExtrinsic> extrinsics, EventList eventsList, ulong blockNumber, CancellationToken stoppingToken)
        {
            uint index = 0;
            foreach (var extrinsic in extrinsics)
            {
                var succeded = eventsList.ExtrinsicSuccess(index);
                index++;

                if (!succeded)
                {
                    continue;
                } 
                
                Func<MarketplaceDbContext, Task>? handler = extrinsic.Extrinsic.Call.Call switch
                {
                    Polkadot.BinaryContracts.Calls.Balances.TransferCall {Dest: var dest, Value: var value} => HandleTransfer(extrinsic, dest, value, blockNumber, stoppingToken),
                    Polkadot.BinaryContracts.Calls.Balances.TransferKeepAliveCall {Dest: var dest, Value: var value} => HandleTransfer(extrinsic, dest, value, blockNumber, stoppingToken),
                    Polkadot.BinaryContracts.Calls.Balances.ForceTransferCall {Dest: var dest, Value: var value} =>  HandleTransfer(extrinsic, dest, value, blockNumber, stoppingToken),
                    _ => null
                };

                if (handler != null)
                {
                    yield return handler;
                }
            }
        }

        private Func<MarketplaceDbContext, Task>? HandleTransfer(DeserializedExtrinsic extrinsic, PublicKey dest, BigInteger value, ulong blockNumber, CancellationToken cancellationToken)
        {
            if (!dest.Bytes.SequenceEqual(_marketplacePublicKey.Bytes))
            {
                return null;
            }

            var sender = extrinsic.Extrinsic.Prefix.Value.AsT1.Address.PublicKey!;
            var senderKey = Convert.ToBase64String(sender.Bytes);
            _logger.LogInformation("Recieved {Amount} kusama from {PublicKey}", value, senderKey);
            return async dbContext =>
            {
                var income = KusamaTransaction.Income(value, senderKey, blockNumber);
                await dbContext.KusamaTransactions.AddAsync(income, cancellationToken);
            };
        }

        public override void Dispose()
        {
            base.Dispose();
            _application?.Dispose();
        }
    }
}