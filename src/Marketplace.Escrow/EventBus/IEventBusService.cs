using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Escrow.EventBus
{
    public interface IEventBusService
    {
        public ValueTask<int> ReadRegisterNft(CancellationToken token);
        public ValueTask PublishRegisterNft(CancellationToken token);
    }
}