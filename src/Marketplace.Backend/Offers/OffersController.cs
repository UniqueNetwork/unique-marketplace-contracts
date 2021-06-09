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
        public Task<PaginationResult<OfferDto>> Get([FromQuery] PaginationParameter paginationParameter, [FromQuery(Name = "collectionId")] List<ulong>? collectionIds = default)
        {
            return _offerService.Get(collectionIds, paginationParameter);
        }

        [HttpGet]
        [Route("{seller}")]
        public Task<PaginationResult<OfferDto>> Get(string seller, [FromQuery] PaginationParameter paginationParameter, [FromQuery(Name = "collectionId")] List<ulong>? collectionIds = default)
        {
            return _offerService.Get(seller, collectionIds, paginationParameter);
        }
    }
}
