using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marketplace.Backend.Offers
{
    public interface IOfferService
    {
        Task<IList<OfferDto>> Get();
        Task<IList<OfferDto>> Get(ulong collectionId);
    }
}