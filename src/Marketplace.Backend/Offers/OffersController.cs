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
        public Task<PaginationResult<OfferDto>> Get([FromQuery] PaginationParameter paginationParameter)
        {
            return _offerService.Get(paginationParameter);
        }

        // [HttpGet]
        // [Route("{collectionId}")]
        // public Task<IList<OfferDto>> Get(ulong collectionId)
        // {
        //     return _offerService.Get(collectionId);
        // }

        [HttpGet]
        [Route("{seller}")]
        public Task<PaginationResult<OfferDto>> Get(string seller, [FromQuery] PaginationParameter paginationParameter)
        {
            return _offerService.Get(seller, paginationParameter);
        }
    }
}
