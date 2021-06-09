using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marketplace.Backend.Offers
{
    public interface IOfferService
    {
        Task<PaginationResult<OfferDto>> Get(IReadOnlyCollection<ulong>? collectionIds, PaginationParameter parameter);
        Task<PaginationResult<OfferDto>> Get(string seller, IReadOnlyCollection<ulong>? collectionIds, PaginationParameter parameter);
    }
}