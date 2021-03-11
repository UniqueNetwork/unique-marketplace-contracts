using System;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db.Models;
using Marketplace.Escrow.DataProcessing;
using Marketplace.Escrow.EventBus;
using Marketplace.Escrow.Extensions;
using Marketplace.Escrow.MatcherContract.Calls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polkadot.Api;
using Polkadot.BinaryContracts.Calls.Contracts;
using Polkadot.DataStructs;

namespace Marketplace.Escrow.RegisterNftDeposit
{
    public class RegisterNftDepositService : DataProcessingService<NftIncomingTransaction>
    {
        private readonly IEventBusService _eventBusService;
        private readonly ILogger<RegisterNftDepositService> _logger;
        private readonly Configuration _configuration;

        public RegisterNftDepositService(IEventBusService eventBusService, IServiceScopeFactory scopeFactory, ILogger<RegisterNftDepositService> logger, Configuration configuration)
            :base(scopeFactory, logger)
        {
            _eventBusService = eventBusService;
            _logger = logger;
            _configuration = configuration;
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                await ScheduleInProgressRun(stoppingToken);
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

        public override Task Process(NftIncomingTransaction transaction)
        {
            return this.CallSubstrate(_logger,
                _configuration.MatcherContractPublicKey, 
                _configuration.UniqueEndpoint,
                new Address() { Symbols = _configuration.MarketplaceUniqueAddress}, 
                _configuration.MarketplacePrivateKeyBytes,
                app => this.ContractCall(app, () => new RegisterNftDepositParameter()
                {
                    User = new PublicKey() {Bytes = transaction.OwnerPublicKeyBytes},
                    CollectionId = transaction.CollectionId,
                    TokenId = transaction.TokenId
                }));
        }
    }
}
