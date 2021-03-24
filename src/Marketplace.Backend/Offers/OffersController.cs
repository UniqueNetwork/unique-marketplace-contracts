using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Marketplace.Backend.Offers
{
    [ApiController]
    [Route("[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        [HttpGet]
        [Route("")]
        public Task<IList<OfferDto>> Get()
        {
            return _offerService.Get();
        }

        // [HttpGet]
        // [Route("{collectionId}")]
        // public Task<IList<OfferDto>> Get(ulong collectionId)
        // {
        //     return _offerService.Get(collectionId);
        // }

        [HttpGet]
        [Route("{seller}")]
        public Task<IList<OfferDto>> Get(string seller)
        {
            return _offerService.Get(seller);
        }
    }
}
