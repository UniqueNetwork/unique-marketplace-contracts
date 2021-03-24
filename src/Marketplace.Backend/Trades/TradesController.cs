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
        public Task<IList<TradeDto>> Get()
        {
            return _tradeService.Get();
        }

        // [HttpGet]
        // [Route("{collectionId}")]
        // public Task<IList<TradeDto>> Get(ulong collectionId)
        // {
        //     return _tradeService.Get(collectionId);
        // }

        [HttpGet]
        [Route("{seller}")]
        public Task<IList<TradeDto>> Get(string seller)
        {
            return _tradeService.Get(seller);
        }

    }
}