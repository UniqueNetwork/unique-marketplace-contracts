using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marketplace.Backend.OnHold
{
    public interface IOnHoldService
    {
        Task<PaginationResult<OnHold>> Get(IReadOnlyCollection<ulong>? collectionIds, PaginationParameter parameter);
        Task<PaginationResult<OnHold>> Get(string owner, IReadOnlyCollection<ulong>? collectionIds, PaginationParameter parameter);
        Task ConnectOffersAndNftIncomes();
    }
}