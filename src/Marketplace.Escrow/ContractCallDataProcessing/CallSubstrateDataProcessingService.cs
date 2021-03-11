using System;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db.Models;
using Marketplace.Escrow.DataProcessing;
using Marketplace.Escrow.Extensions;
using Marketplace.Escrow.MatcherContract.Calls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polkadot.Api;
using Polkadot.BinaryContracts;
using Polkadot.BinaryContracts.Calls.Contracts;
using Polkadot.BinarySerializer;
using Polkadot.DataStructs;

namespace Marketplace.Escrow.ContractCallDataProcessing
{
    public abstract class CallSubstrateDataProcessingService<TModel>  : DataProcessingService<TModel> where TModel : class, IDataToProcess
    {
        private readonly ILogger _logger;

        protected CallSubstrateDataProcessingService(IServiceScopeFactory scopeFactory, ILogger logger) : base(scopeFactory, logger)
        {
            _logger = logger;
        }
    }
}