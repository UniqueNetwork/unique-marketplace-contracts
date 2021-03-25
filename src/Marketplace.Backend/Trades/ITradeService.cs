using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marketplace.Backend.Trades
{
    public interface ITradeService
    {
        Task<PaginationResult<TradeDto>> Get(PaginationParameter parameter);
        Task<IList<TradeDto>> Get(ulong collectionId);
        Task<PaginationResult<TradeDto>> Get(string seller, PaginationParameter parameter);
    }
}