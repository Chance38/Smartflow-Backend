using Microsoft.AspNetCore.Mvc;
using Middleware;
using SmartFlowBackend.Application.SwaggerSetting;
using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Application.Controller
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

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to get balance for user {UserId}", userId);

            try
            {
                var balance = await _balanceService.GetBalanceByUserIdAsync(userId);
                return Ok(new GetBalanceResponse
                {
                    RequestId = requestId,
                    Balance = balance
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ClientErrorSituation
                {
                    RequestId = requestId,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}

