using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db;
using Marketplace.Db.Models;
using Marketplace.Escrow.TransactionScanner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polkadot.BinaryContracts.Calls.Contracts;
using Polkadot.BinaryContracts.Calls.Nft;
using Polkadot.DataStructs;

namespace Marketplace.Escrow.UniqueScanner
{
    public class UniqueBlockScannerService : ExtrinsicBlockScannerService<UniqueProcessedBlock>
    {
        private readonly Configuration _configuration;

        public UniqueBlockScannerService(IServiceScopeFactory scopeFactory, ILogger<UniqueBlockScannerService> logger, Configuration configuration) : base(scopeFactory, logger, configuration.UniqueEndpoint, IsolationLevel.RepeatableRead)
        {
            _configuration = configuration;
        }

        protected override IEnumerable<Func<MarketplaceDbContext, Task>> ProcessExtrinsics(IEnumerable<DeserializedExtrinsic> extrinsics, ulong blockNumber, CancellationToken stoppingToken)
        {
            foreach (var extrinsic in extrinsics)
            {
                Func<MarketplaceDbContext, Task>? handler = extrinsic.Extrinsic.Call.Call switch
                {
                    CallCall c => HandleContractCall(c),
                    TransferCall t => HandleTransfer(t, extrinsic.Extrinsic.Prefix.Value.AsT1.Address.PublicKey, blockNumber),
                    _ => null
                };

                if (handler != null)
                {
                    yield return handler;
                }
            }
        }

        private Func<MarketplaceDbContext, Task>? HandleTransfer(TransferCall transferCall, PublicKey sender, ulong blockNumber)
        {
            if (!transferCall.Recipient.Bytes.SequenceEqual(_configuration.MarketplaceUniquePublicKey.Bytes))
            {
                return null;
            }

            return async dbContext =>
            {
                await dbContext.NftIncomeTransactions.AddAsync(new NftIncomeTransaction()
                {
                    Deposited = false,
                    Id = Guid.NewGuid(),
                    CollectionId = transferCall.CollectionId,
                    TokenId = transferCall.ItemId,
                    OwnerPublicKeyBytes = sender.Bytes,
                    UniqueProcessedBlockId = blockNumber,
                    Value = transferCall.Value,
                });
            };
        }

        private Func<MarketplaceDbContext, Task>? HandleContractCall(CallCall callCall)
        {
            throw new NotImplementedException();
        }
    }
}