using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marketplace.Backend.Offers
{
    public interface IOfferService
    {
        Task<PaginationResult<OfferDto>> Get(ulong? collectionId, PaginationParameter parameter);
        Task<PaginationResult<OfferDto>> Get(string seller, ulong? collectionId, PaginationParameter parameter);
    }
}