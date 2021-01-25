using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marketplace.Backend.Trades
{
    public interface ITradeService
    {
        Task<IList<TradeDto>> Get();
        Task<IList<TradeDto>> Get(ulong collectionId);
    }
}