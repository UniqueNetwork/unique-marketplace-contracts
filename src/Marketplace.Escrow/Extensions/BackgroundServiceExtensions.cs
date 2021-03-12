using System;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db.Models;
using Marketplace.Escrow.DataProcessing;
using Marketplace.Escrow.Extensions;
using Marketplace.Escrow.MatcherContract.Calls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polkadot.Api;
using Polkadot.BinaryContracts;
using Polkadot.BinaryContracts.Calls.Contracts;
using Polkadot.BinarySerializer;
using Polkadot.DataStructs;

namespace Marketplace.Escrow.Extensions
{
    public static class BackgroundServiceExtensions
    {
        public static Task CallSubstrate(this BackgroundService service, ILogger logger, PublicKey contractKey, string nodeEndpoint, Address from, byte[] privateKey, Func<IApplication, IExtrinsicCall> callGenerator)
        {
            var completionSource = new TaskCompletionSource();

            Task.Run(async () =>
            {
                SafeApplication application = SafeApplication.CreateApplication(
                    ex =>
                    {
                        logger.LogError(ex, "{ServiceName} substrate api failed", service.GetType().FullName);
                        Interlocked.Exchange<TaskCompletionSource?>(ref completionSource, null)?.SetException(ex);
                    }, logger, contractKey);
                try
                {

                    application.Application.Connect(nodeEndpoint);
                    var call = callGenerator(application.Application!);
                    application.HealthCheck(TimeSpan.FromMinutes(10), () =>
                    {
                        Interlocked.Exchange<TaskCompletionSource?>(ref completionSource, null)?.SetException(new TimeoutException());
                        application.Dispose();
                    });
                    var result =
                        await application.Application.SignWaitRetryOnLowPriority(from,
                            privateKey, call);
                    application.CancelHealthCheck();
                    result.Switch(_ =>
                    {
                        Interlocked.Exchange<TaskCompletionSource?>(ref completionSource, null)?.SetResult();
                    }, fail =>
                    {
                        var error = fail.ErrorMessage(application.Application);
                        logger.LogError("Failed to register NFT via contract, {ErrorText}", error);
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

        public static IExtrinsicCall ContractCall(this BackgroundService service, IApplication application,
            Func<IContractCallParameter> contractParameter)
        {
            var parameter = contractParameter();
            var call = CallCall.Create(0, 200000000000, parameter, application.Serializer);
            return call;
        }

    }
}