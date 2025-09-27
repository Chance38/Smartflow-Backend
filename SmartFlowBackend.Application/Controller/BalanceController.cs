using Microsoft.AspNetCore.Mvc;
using Middleware;
using Domain.Contract;
using Domain.Interface;

namespace Application.Controller
{
    [ApiController]
    [Route("smartflow/v1")]
    public class BalanceController : ControllerBase
    {
        private readonly IBalanceService _balanceService;
        private readonly ILogger<BalanceController> _logger;

        public BalanceController(IBalanceService balanceService, ILogger<BalanceController> logger)
        {
            _balanceService = balanceService;
            _logger = logger;
        }

        [HttpGet("balance")]
        [ProducesResponseType(typeof(GetBalanceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBalance()
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            var userId = ServiceMiddleware.GetUserId(HttpContext);
            _logger.LogInformation("Received request to get balance for user {UserId}", userId);

            try
            {
                var balance = await _balanceService.GetBalanceAsync(userId);
                return Ok(new GetBalanceResponse
                {
                    RequestId = requestId,
                    Balance = balance
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "This Should never happen. Balance Form User: {UserId} doesn't create.", userId);
                return BadRequest(new ClientErrorSituation
                {
                    RequestId = requestId,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}

