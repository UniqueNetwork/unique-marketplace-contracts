using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marketplace.Backend.Trades
{
    public interface ITradeService
    {
        Task<PaginationResult<TradeDto>> Get(IReadOnlyCollection<ulong>? collectionIds, PaginationParameter parameter);
        Task<PaginationResult<TradeDto>> Get(string seller, IReadOnlyCollection<ulong>? collectionIds, PaginationParameter parameter);
    }
}