using System;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db.Models;
using Marketplace.Escrow.ContractCallDataProcessing;
using Marketplace.Escrow.DataProcessing;
using Marketplace.Escrow.Extensions;
using Marketplace.Escrow.MatcherContract.Calls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polkadot.Api;
using Polkadot.BinaryContracts;
using Polkadot.BinaryContracts.Calls.Contracts;
using Polkadot.DataStructs;
using Polkadot.Utils;

namespace Marketplace.Escrow.RegisterKusamaDeposit
{
    public class RegisterQuoteDepositService : CallSubstrateDataProcessingService<QuoteIncomeTransaction>
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

        private void RunInterval(CancellationToken stoppingToken)
        {
            Task.Run(async () =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    await Run(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ServiceName} failed", GetType().FullName);
                }
                
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                RunInterval(stoppingToken);
            }, stoppingToken);
        }

        public override Task Process(QuoteIncomeTransaction quoteIncome)
        {
            return this.CallSubstrate(_logger,
                _configuration.MatcherContractPublicKey, 
                _configuration.UniqueEndpoint,
                new Address() { Symbols = _configuration.MarketplaceUniqueAddress}, 
                _configuration.MarketplacePrivateKeyBytes,
                app => this.ContractCall(app, () => new RegisterDepositParameter()
                {
                    User = new PublicKey() {Bytes = quoteIncome.AccountPublicKeyBytes},
                    DepositBalance = new Balance() {Value = quoteIncome.Amount},
                    QuoteId = quoteIncome.QuoteId
                }));
        }
    }
}