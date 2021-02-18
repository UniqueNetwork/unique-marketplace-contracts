using System;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Escrow.ApiLogger;
using Microsoft.Extensions.Logging;
using Polkadot.Api;

namespace Marketplace.Escrow
{
    public class SafeApplication : IDisposable
    {
        private Application? _application;
        private Task? _healthCheckTask;
        private int _disposed = 0;

        public Application? Application => _application;

        public SafeApplication(Application application)
        {
            _application = application;
        }


        public static SafeApplication CreateApplication(Action<Exception> onError, ILogger logger)
        {
            var param = new JsonRpcParams {JsonrpcVersion = "2.0"};

            var substrateLogger = new SubstrateApiLogger(logger);
            var jsonRpc = new JsonRpc(new Wsclient(substrateLogger), substrateLogger, param, onError);

            return new SafeApplication(new Application(substrateLogger, jsonRpc, Application.DefaultSubstrateSettings()));
        }

        public void HealthCheck(TimeSpan timeout, Action onHealthCheck)
        {
            if (_disposed != 0)
            {
                return;
            }

            _healthCheckTask = Task.Delay(timeout)
                .ContinueWith(t =>
                {
                    if (t == _healthCheckTask && _disposed == 0)
                    {
                        onHealthCheck();
                    }
                });
        }

        public void CancelHealthCheck()
        {
            _healthCheckTask = null;
        }
        
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }
            
            _healthCheckTask = null;
            Interlocked.Exchange(ref _application, null)?.Dispose();
        }
    }
}