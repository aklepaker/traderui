using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using traderui.Server.Commands;
using traderui.Server.IBKR;
using traderui.Shared.Requests;

namespace traderui.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrokerController : ControllerBase
    {
        private readonly ILogger<BrokerController> _logger;
        private readonly IInteractiveBrokers _broker;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public BrokerController(ILogger<BrokerController> logger,
            IInteractiveBrokers broker,
            IMediator mediator,
            IMapper mapper)
        {
            _logger = logger;
            _broker = broker;
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpGet("ticker/{symbol}")]
        public IActionResult Get(string symbol)
        {
            _mediator.Send(new GetTickerCommand
            {
                Symbol = symbol,
            });

            return Ok();
        }

        [HttpGet("ticker/{symbol}/price")]
        public IActionResult UpdatePrice(string symbol)
        {
            _mediator.Send(new GetTickerPriceCommand
            {
                Symbol = symbol,
            });
            return Ok();
        }

        [HttpGet("ticker/{symbol}/historic")]
        public IActionResult GetHistoricData(string symbol)
        {
            _mediator.Send(new GetHistoricDataCommand
            {
                Symbol = symbol,
            });
            return Ok();
        }

        [HttpGet("ticker/{symbol}/historicbardata/{requestId}")]
        public IActionResult GetHistoricalBarData(string symbol, int requestId)
        {
            _mediator.Send(new GetHistoricalBarDataCommand
            {
                Symbol = symbol, RequestId = requestId,
            });
            return Ok();
        }

        [HttpPost("ticker/{name}/buy")]
        public IActionResult PlaceOrder(string name, [FromBody] PlaceOrderRequest placeOrderRequest)
        {
            try
            {
                var placeOrderCommand = _mapper.Map<PlaceOrderRequest, PlaceOrderCommand>(placeOrderRequest);
                _mediator.Send(placeOrderCommand);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }

            return Ok();
        }

        [HttpGet("account/summary/{stopRequest}")]
        public IActionResult GetAccountSummary(bool stopRequest = false)
        {
            _mediator.Send(new GetAccountSummaryCommand
            {
                StopRequest = stopRequest,
            });
            return Ok();
        }

        [HttpGet("account/positions")]
        public IActionResult GetPositions()
        {
            _mediator.Send(new GetPositionsCommand());
            return Ok();
        }

        [HttpGet("account/pnl/{account}")]
        public IActionResult GetPnL(string account)
        {
            _mediator.Send(new GetPnLCommand
            {
                Account = account,
            });
            return Ok();
        }

        [HttpGet("account/pnl/{account}/{conId}/{active}")]
        public IActionResult GetPnL(string account, int conId, bool active)
        {
            _mediator.Send(new GetPnLCommand
            {
                Account = account, ContractId = conId, Active = active,
            });
            return Ok();
        }

        [HttpGet("api/broker/cancelSubscriptions")]
        public IActionResult CancelSubscriptions()
        {
            _mediator.Send(new CancelSubscriptionsCommand());
            return Ok();
        }
    }
}
