using System.Linq;
using Newtonsoft.Json;
using Polkadot.Api;
using Polkadot.BinaryContracts.Events.System;

namespace Marketplace.Escrow.Extensions
{
    public static class ExtrinsicFailedExtensions
    {
        public static string ErrorMessage(this ExtrinsicFailed fail, IApplication application)
        {
            var error = fail.EventArgument0.Value.Match(
                other => $"Other: {JsonConvert.SerializeObject(other)}",
                lookup => $"CannotLookup: {JsonConvert.SerializeObject(lookup)}",
                badOrigin => $"BadOrigin: {JsonConvert.SerializeObject(badOrigin)}",
                module =>
                {
                    var meta = application.GetMetadata(null);
                    var moduleError = meta.GetModules().ElementAtOrDefault(module.Index)?.GetErrors()
                        ?.ElementAtOrDefault(module.Error);
                    return
                        $"Module Error: {moduleError?.GetName() ?? ""}, index: {module.Index}, error: {module.Error}";
                });
            return error;
        }
    }
}