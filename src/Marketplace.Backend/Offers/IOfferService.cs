using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marketplace.Backend.Offers
{
    public interface IOfferService
    {
        Task<PaginationResult<OfferDto>> Get(PaginationParameter parameter);
        Task<IList<OfferDto>> Get(ulong collectionId);
        Task<PaginationResult<OfferDto>> Get(string seller, PaginationParameter parameter);
    }
}