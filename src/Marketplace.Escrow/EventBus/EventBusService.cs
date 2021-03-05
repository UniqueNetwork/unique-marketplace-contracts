using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Marketplace.Escrow.EventBus
{
    public class EventBusService : IEventBusService
    {
        private readonly Channel<int> _registerNftDeposit = Channel.CreateUnbounded<int>();
        
        public ValueTask<int> ReadRegisterNft(CancellationToken token)
        {
            return _registerNftDeposit.Reader.ReadAsync(token);
        }

        public ValueTask PublishRegisterNft(CancellationToken token)
        {
            return _registerNftDeposit.Writer.WriteAsync(0, token);
        }
    }
}