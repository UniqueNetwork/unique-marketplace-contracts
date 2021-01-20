using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Marketplace.Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MarketplaceController : ControllerBase
    {
        private readonly ILogger<MarketplaceController> _logger;

        public MarketplaceController(ILogger<MarketplaceController> logger)
        {
            _logger = logger;
        }
    }
}
