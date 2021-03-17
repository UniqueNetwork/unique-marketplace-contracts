using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db.Models;
using Marketplace.Escrow.DataProcessing;
using Marketplace.Escrow.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polkadot.BinaryContracts.Calls.Nft;
using Polkadot.DataStructs;
using Polkadot.Utils;

namespace Marketplace.Escrow.TransferSoldNft
{
    public class TransferSoldNftService : DataProcessingService<NftOutgoingTransaction>
    {
        private readonly ILogger<TransferSoldNftService> _logger;
        private readonly Configuration _configuration;

        public TransferSoldNftService(IServiceScopeFactory scopeFactory, ILogger<TransferSoldNftService> logger, Configuration configuration) : base(scopeFactory, logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RunInterval(stoppingToken);
            return Task.CompletedTask;
        }

        public override async Task Process(NftOutgoingTransaction outgoing)
        {
            var account = AddressUtils.GetAddrFromPublicKey(new PublicKey() {Bytes = outgoing.RecipientPublicKeyBytes});
            _logger.LogInformation("Calling Nft.Transfer({Account}, {CollectionId}, {TokenId}, {Value})", account, outgoing.CollectionId, outgoing.TokenId, outgoing.Value);
            var recipient = new PublicKey() {Bytes = outgoing.RecipientPublicKeyBytes};
            await this.CallSubstrate(
                _logger,
                _configuration.MatcherContractPublicKey,
                _configuration.UniqueEndpoint,
                new Address(_configuration.MarketplaceUniqueAddress),
                _configuration.MarketplacePrivateKeyBytes,
                app => new TransferCall(recipient, (uint) outgoing.CollectionId, (uint) outgoing.TokenId, outgoing.Value));
            _logger.LogInformation("Successfully called Nft.Transfer({Account}, {CollectionId}, {TokenId}, {Value})", account, outgoing.CollectionId, outgoing.TokenId, outgoing.Value);
        }
    }
}