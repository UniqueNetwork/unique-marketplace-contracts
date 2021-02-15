using Microsoft.Extensions.Logging;
using IPolkaLogger = Polkadot.ILogger;

namespace Marketplace.Escrow.ApiLogger
{
    public class SubstrateApiLogger: IPolkaLogger
    {
        private readonly ILogger _logger;

        // ReSharper disable once ContextualLoggerProblem
        public SubstrateApiLogger(ILogger logger)
        {
            _logger = logger;
        }
        
        public void Info(string message)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogInformation(message);
        }

        public void Error(string message)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogError(message);
        }

        public void Warning(string message)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogWarning(message);
        }
    }
}