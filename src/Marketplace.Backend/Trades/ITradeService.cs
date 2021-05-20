using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marketplace.Backend.Trades
{
    public interface ITradeService
    {
        Task<PaginationResult<TradeDto>> Get(ulong? collectionId, PaginationParameter parameter);
        Task<PaginationResult<TradeDto>> Get(string seller, ulong? collectionId, PaginationParameter parameter);
    }
}