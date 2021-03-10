using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Marketplace.Db;
using Marketplace.Db.Migrations;
using Marketplace.Db.Models;
using Marketplace.Escrow.DataProcessing;
using Marketplace.Escrow.EventBus;
using Marketplace.Escrow.MatcherContract.Calls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polkadot.Api;
using Polkadot.BinaryContracts.Calls.Contracts;
using Polkadot.BinaryContracts.Events;
using Polkadot.BinaryContracts.Events.System;
using Polkadot.Data;
using Polkadot.DataStructs;
using Polkadot.Utils;

namespace Marketplace.Escrow.RegisterNftDeposit
{
    public class RegisterNftDepositService : DataProcessingService<NftIncomeTransaction>
    {
        private readonly IEventBusService _eventBusService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RegisterNftDepositService> _logger;
        private readonly Configuration _configuration;

        public RegisterNftDepositService(IEventBusService eventBusService, IServiceScopeFactory scopeFactory, ILogger<RegisterNftDepositService> logger, Configuration configuration)
            :base(scopeFactory, logger)
        {
            _eventBusService = eventBusService;
            _scopeFactory = scopeFactory;
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

        public override Task Process(NftIncomeTransaction transaction)
        {
            var completionSource = new TaskCompletionSource();

            Task.Run(async () =>
            {
                SafeApplication application = SafeApplication.CreateApplication(
                    ex =>
                    {
                        _logger.LogError(ex, "{ServiceName} substrate api failed", GetType().FullName);
                        Interlocked.Exchange<TaskCompletionSource?>(ref completionSource, null)?.SetException(ex);
                    }, _logger, _configuration.MatcherContractPublicKey);
                try
                {

                    application.Application.Connect(_configuration.UniqueEndpoint);
                    var address = new Address(_configuration.MarketplaceUniqueAddress);
                    var parameter = new RegisterNftDepositParameter()
                    {
                        User = new PublicKey() {Bytes = transaction.OwnerPublicKeyBytes},
                    };
                    var call = CallCall.Create(0, 200000000000, parameter, application.Application!.Serializer);
                    application.HealthCheck(TimeSpan.FromMinutes(10), () =>
                    {
                        Interlocked.Exchange<TaskCompletionSource?>(ref completionSource, null)?.SetException(new TimeoutException());
                        application.Dispose();
                    });
                    var result =
                        await application.Application.SignAndWaitForResult(address,
                            _configuration.MarketplacePrivateKeyBytes, call);
                    application.CancelHealthCheck();
                    result.Switch(_ =>
                    {
                        Interlocked.Exchange<TaskCompletionSource?>(ref completionSource, null)?.SetResult();
                    }, fail =>
                    {
                        var error = fail.EventArgument0.Value.Match(
                            other => $"Other: {JsonConvert.SerializeObject(other)}",
                            lookup => $"CannotLookup: {JsonConvert.SerializeObject(lookup)}",
                            badOrigin => $"BadOrigin: {JsonConvert.SerializeObject(badOrigin)}",
                            module =>
                            {
                                var meta = application.Application.GetMetadata(null);
                                var moduleError = meta.GetModules().ElementAtOrDefault(module.Index)?.GetErrors()
                                    ?.ElementAtOrDefault(module.Error);
                                return
                                    $"Module Error: {moduleError?.GetName() ?? ""}, index: {module.Index}, error: {module.Error}";
                            });
                        _logger.LogError("Failed to register NFT via contract, {ErrorText}", error);
                        Interlocked.Exchange<TaskCompletionSource?>(ref completionSource, null)?.SetException(new ApplicationException(error));
                    });
                }
                catch (Exception ex)
                {
                    Interlocked.Exchange<TaskCompletionSource?>(ref completionSource, null)?.SetException(ex);
                }
                finally
                {
                    application.Dispose();
                }

                
            });
            return completionSource.Task;
        }
    }
}
