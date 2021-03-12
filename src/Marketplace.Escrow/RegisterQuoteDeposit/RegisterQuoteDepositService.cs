using System;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db.Models;
using Marketplace.Escrow.DataProcessing;
using Marketplace.Escrow.Extensions;
using Marketplace.Escrow.MatcherContract.Calls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polkadot.BinaryContracts;
using Polkadot.DataStructs;

namespace Marketplace.Escrow.RegisterQuoteDeposit
{
    public class RegisterQuoteDepositService : DataProcessingService<QuoteIncomingTransaction>
    {
        private readonly ILogger _logger;
        private readonly Configuration _configuration;

        public RegisterQuoteDepositService(IServiceScopeFactory scopeFactory, ILogger<RegisterQuoteDepositService> logger, Configuration configuration) : base(scopeFactory, logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RunInterval(stoppingToken);
            return Task.CompletedTask;
        }

        public override Task Process(QuoteIncomingTransaction quoteIncoming)
        {
            return this.CallSubstrate(_logger,
                _configuration.MatcherContractPublicKey, 
                _configuration.UniqueEndpoint,
                new Address() { Symbols = _configuration.MarketplaceUniqueAddress}, 
                _configuration.MarketplacePrivateKeyBytes,
                app => this.ContractCall(app, () => new RegisterDepositParameter()
                {
                    User = new PublicKey() {Bytes = quoteIncoming.AccountPublicKeyBytes},
                    DepositBalance = new Balance() {Value = quoteIncoming.Amount},
                    QuoteId = quoteIncoming.QuoteId
                }));
        }
    }
}