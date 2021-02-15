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
        private Application? _application = null;
        private PublicKey _marketplacePublicKey;
        private TaskCompletionSource<int>? _runningTask = null;

        public KusamaScannerService(IServiceScopeFactory scopeFactory, ILogger<KusamaScannerService> logger, Configuration configuration): base(logger, scopeFactory, configuration.KusamaEndpoint)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
            _marketplacePublicKey = configuration.MarketplaceKusamaPublicKey;
        }

        protected override Task ProcessBlock(ulong blockNumber, CancellationToken stoppingToken)
        {
            if (_runningTask != null)
            {
                throw new Exception("Something wrong, can't run multiple instances.");
            }
            _runningTask = new ();
            var task = _runningTask.Task;
            if (_application == null)
            {
                _application = CreateApplication(ex =>
                {
                    _logger.LogError(ex, "Kusama listener failed");
                    _runningTask.SetException(ex);
                    _runningTask = null;
                    _application?.Dispose();
                    _application = null;
                });
                _application.Connect(_configuration.KusamaEndpoint);
            }

            var myTask = _runningTask;

            Run(blockNumber, stoppingToken).ContinueWith(t =>
            {
                if (myTask != _runningTask)
                {
                    return;
                }
                if (t.IsCanceled)
                {
                    _runningTask.SetCanceled();
                }

                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "Kusama listener failed");
                    _runningTask.SetException(t.Exception!);
                }

                if (t.IsCompletedSuccessfully)
                {
                    _runningTask.SetResult(0);
                }
                _runningTask = null;
            });
            
            return task;
        }

        private async Task Run(ulong blockNumber, CancellationToken stoppingToken)
        {
            var blockHash = _application!.GetBlockHash(new GetBlockHashParams() {BlockNumber = blockNumber}).Hash;
            var block = _application.GetBlock(new GetBlockParams() {BlockHash = blockHash});
            var eventsString =
                _application.StorageApi.GetStorage("system", "events", new GetBlockHashParams() {BlockNumber = blockNumber});
            var eventsList = _application.Serializer.Deserialize<EventList>(eventsString.ToByteArray());
            var extrinsics = block.Block.Extrinsic
                .Select(e => _application.Serializer.DeserializeAssertReadAll<DeserializedExtrinsic>(e!.ToByteArray()));

            var actions = ProcessExtrinsics(extrinsics, eventsList, blockNumber, stoppingToken).ToList();

            stoppingToken.ThrowIfCancellationRequested();
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
            await using var transaction = await dbContext!.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, stoppingToken);
            try
            {
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