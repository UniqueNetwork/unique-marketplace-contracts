using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Marketplace.Backend.OnHold
{
    [ApiController]
    [Route("[controller]")]
    public class OnHoldController : ControllerBase
    {
        private readonly IOnHoldService _onHoldService;

        public OnHoldController(IOnHoldService onHoldService)
        {
            _onHoldService = onHoldService;
        }

        [HttpGet]
        [Route("")]
        public Task<PaginationResult<OnHold>> Get([FromQuery] PaginationParameter paginationParameter, [FromQuery(Name = "collectionId")] List<ulong>? collectionIds = default)
        {
            return _onHoldService.Get(collectionIds, paginationParameter);
        }

        [HttpGet]
        [Route("{owner}")]
        public Task<PaginationResult<OnHold>> Get(string owner, [FromQuery] PaginationParameter paginationParameter, [FromQuery(Name = "collectionId")] List<ulong>? collectionIds = default)
        {
            return _onHoldService.Get(owner, collectionIds, paginationParameter);
        }

        [HttpGet]
        [Route("[action]")]
        public Task ConnectOffersAndNftIncomes()
        {
            return _onHoldService.ConnectOffersAndNftIncomes();
        }
    }
}
