using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Backend.Trades
{
    [ApiController]
    [Route("[controller]")]
    public class TradesController : ControllerBase
    {
        private readonly ITradeService _tradeService;

        public TradesController(ITradeService tradeService)
        {
            _tradeService = tradeService;
        }
        
        
        [HttpGet]
        [Route("")]
        public Task<PaginationResult<TradeDto>> Get([FromQuery] PaginationParameter parameter, [FromQuery(Name = "collectionId")] List<ulong>? collectionIds = default)
        {
            return _tradeService.Get(collectionIds, parameter);
        }

        [HttpGet]
        [Route("{seller}")]
        public Task<PaginationResult<TradeDto>> Get(string seller, [FromQuery] PaginationParameter parameter, [FromQuery(Name = "collectionId")] List<ulong>?  collectionIds = default)
        {
            return _tradeService.Get(seller, collectionIds, parameter);
        }

    }
}