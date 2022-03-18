using Microsoft.AspNetCore.Mvc;
using traderui.Server.IBKR;
using traderui.Shared;
using traderui.Shared.Models;

namespace traderui.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrokerController : ControllerBase
    {
        private readonly ILogger<BrokerController> _logger;
        private readonly InteractiveBrokers _broker;

        public BrokerController(ILogger<BrokerController> logger, InteractiveBrokers broker)
        {
            _logger = logger;
            _broker = broker;
        }

        [HttpGet("ticker/{name}")]
        public IActionResult Get(string name)
        {
            _broker.GetTicker(name);
            return Ok(new Ticker {Name = $"{name}-result"});
        }

        [HttpGet("ticker/{name}/price")]
        public IActionResult UpdatePrice(string name)
        {
            _broker.GetTickerPrice(name);
            return Ok();
        }

        [HttpGet("ticker/{name}/historic")]
        public IActionResult GetHistoricData(string name)
        {
            _broker.GetHistoricPrice(name);
            return Ok();
        }

        [HttpGet("ticker/{name}/historicbardata/{requestId}")]
        public IActionResult GetHistoricalBarData(string name, int requestId)
        {
            _broker.GetHistoricBarData(name, requestId);
            return Ok();
        }

        [HttpPost("ticker/{name}/buy")]
        public IActionResult PlaceOrder(string name, [FromBody] WebOrder order)
        {
            _broker.PlaceOrder(order);
            return Ok();
        }

        [HttpGet("account/summary/{stopRequest}")]
        public IActionResult GetAccountSummary(bool stopRequest = false)
        {
            _broker.GetAccountSummary(stopRequest);
            return Ok();
        }

        [HttpGet("account/positions")]
        public IActionResult GetPositions()
        {
            _broker.GetPositions();
            return Ok();
        }

        [HttpGet("account/pnl/{account}")]
        public IActionResult GetPnL(string account)
        {
            _broker.GetPnL(account);
            return Ok();
        }

        [HttpGet("account/pnl/{account}/{conId}/{active}")]
        public IActionResult GetPnL(string account, int conId, bool active)
        {
            _broker.GetTickerPnL(account, conId, active);
            return Ok();
        }

        [HttpGet("api/broker/cancelSubscriptions")]
        public IActionResult CancelSubscriptions()
        {
            _broker.CancelSubscriptions();
            return Ok();
        }
    }
}
