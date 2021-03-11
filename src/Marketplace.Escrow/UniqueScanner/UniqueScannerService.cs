using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db;
using Marketplace.Db.Models;
using Marketplace.Escrow.EventBus;
using Marketplace.Escrow.MatcherContract.Calls;
using Marketplace.Escrow.TransactionScanner;
using Microsoft.EntityFrameworkCore;
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
        private readonly IEventBusService _eventBusService;

        public UniqueBlockScannerService(IServiceScopeFactory scopeFactory, ILogger<UniqueBlockScannerService> logger, Configuration configuration, IEventBusService eventBusService) : base(scopeFactory, logger, configuration.UniqueEndpoint, IsolationLevel.RepeatableRead, configuration.MatcherContractPublicKey)
        {
            _configuration = configuration;
            _eventBusService = eventBusService;
        }

        protected override IEnumerable<ExtrinsicHandler> ProcessExtrinsics(IEnumerable<DeserializedExtrinsic> extrinsics, ulong blockNumber, CancellationToken stoppingToken)
        {
            foreach (var extrinsic in extrinsics)
            {
                Func<MarketplaceDbContext, ValueTask>? handler = extrinsic.Extrinsic.Call.Call switch
                {
                    CallCall c => HandleContractCall(c, extrinsic.Extrinsic.Prefix.Value.AsT1.Address.PublicKey),
                    TransferCall t => HandleTransfer(t, extrinsic.Extrinsic.Prefix.Value.AsT1.Address.PublicKey, blockNumber),
                    _ => null
                };

                if (handler != null)
                {
                    yield return new ExtrinsicHandler()
                    {
                        OnSaveToDb = handler,
                        OnAfterSaveToDb = () => _eventBusService.PublishRegisterNft(stoppingToken),
                    };
                }
            }
        }

        private Func<MarketplaceDbContext, ValueTask>? HandleTransfer(TransferCall transferCall, PublicKey sender, ulong blockNumber)
        {
            if (!transferCall.Recipient.Bytes.SequenceEqual(_configuration.MarketplaceUniquePublicKey.Bytes))
            {
                return null;
            }

            return async dbContext =>
            {
                await dbContext.NftIncomeTransactions.AddAsync(new NftIncomeTransaction()
                {
                    Status = ProcessingDataStatus.InProgress,
                    Id = Guid.NewGuid(),
                    CollectionId = transferCall.CollectionId,
                    TokenId = transferCall.ItemId,
                    OwnerPublicKeyBytes = sender.Bytes,
                    UniqueProcessedBlockId = blockNumber,
                    Value = transferCall.Value,
                });
            };
        }

        private Func<MarketplaceDbContext, ValueTask>? HandleContractCall(CallCall callCall, PublicKey sender)
        {
            if (!callCall.Dest.Bytes.SequenceEqual(_configuration.MatcherContractPublicKey.Bytes))
            {
                return null;
            }

            return callCall.Parameters switch
            {
                AskParameter a => HandleAsk(a, sender),
                BuyParameter b => HandleBuy(b, sender),
                _ => null
            };
        }

        private Func<MarketplaceDbContext, ValueTask>? HandleBuy(BuyParameter buyParameter, PublicKey sender)
        {
            return async dbContext =>
            {
                var offer = await dbContext.Offers.FirstAsync(o =>
                    buyParameter.CollectionId == o.CollectionId && buyParameter.TokenId == o.TokenId && o.OfferStatus == OfferStatus.Active);
                offer.OfferStatus = OfferStatus.Traded;

                await dbContext.Trades.AddAsync(new Trade()
                {
                    BuyerPublicKey = sender.Bytes,
                    Id = Guid.NewGuid(),
                    Offer = offer,
                    TradeDate = DateTime.UtcNow,
                });

                await dbContext.KusamaOutgoingTransactions.AddAsync(new QuoteOutgoingTransaction()
                {
                    Id = Guid.NewGuid(),
                    Status = ProcessingDataStatus.InProgress,
                    Value = offer.Price,
                    QuoteId = offer.QuoteId,
                    RecipientPublicKeyBytes = offer.SellerPublicKeyBytes
                });

                await dbContext.NftOutgoingTransactions.AddAsync(new NftOutgoingTransaction()
                {
                    Id = Guid.NewGuid(),
                    Status = ProcessingDataStatus.InProgress,
                    Value = BigInteger.Zero,
                    CollectionId = offer.CollectionId,
                    TokenId = offer.TokenId,
                    RecipientPublicKeyBytes = sender.Bytes,
                });

                await dbContext.SaveChangesAsync();
            };
        }

        private Func<MarketplaceDbContext, ValueTask>? HandleAsk(AskParameter askParameter, PublicKey sender)
        {
            return async dbContext =>
            {
                await dbContext.Offers.AddAsync(new Offer()
                {
                    Id = Guid.NewGuid(),
                    Price = askParameter.Price.Value,
                    CollectionId = askParameter.CollectionId,
                    TokenId = askParameter.TokenId,
                    CreationDate = DateTime.UtcNow,
                    OfferStatus = OfferStatus.Active,
                    SellerPublicKeyBytes = sender.Bytes,
                    Metadata = "",
                });
                await dbContext.SaveChangesAsync();
            };
        }
    }
}