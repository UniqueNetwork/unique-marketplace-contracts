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
using Marketplace.Escrow.TransactionScanner;
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
    public class KusamaBlockScannerService: ExtrinsicBlockScannerService<KusamaProcessedBlock>
    {
        private readonly ILogger<KusamaBlockScannerService> _logger;
        private readonly Configuration _configuration;
        private SafeApplication? _application = null;
        private PublicKey _marketplacePublicKey;

        public KusamaBlockScannerService(IServiceScopeFactory scopeFactory, ILogger<KusamaBlockScannerService> logger, Configuration configuration): base(scopeFactory, logger, configuration.KusamaEndpoint, IsolationLevel.RepeatableRead)
        {
            _logger = logger;
            _configuration = configuration;
            _marketplacePublicKey = configuration.MarketplaceKusamaPublicKey;
        }

        protected override IEnumerable<Func<MarketplaceDbContext, Task>> ProcessExtrinsics(IEnumerable<DeserializedExtrinsic> extrinsics, ulong blockNumber, CancellationToken stoppingToken)
        {
            uint index = 0;
            foreach (var extrinsic in extrinsics)
            {
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
            _logger.LogInformation("Recieved {Amount} kusama from {PublicKey}", value, sender);
            return async dbContext =>
            {
                var income = KusamaTransaction.Income(value, sender.Bytes, blockNumber);
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