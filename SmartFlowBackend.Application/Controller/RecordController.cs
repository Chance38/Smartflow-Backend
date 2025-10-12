using Microsoft.AspNetCore.Mvc;
using Middleware;
using Domain.Contract;
using Domain.Interface;

namespace Application.Controller
{
    [ApiController]
    [Route("smartflow/v1")]
    public class RecordController : ControllerBase
    {
        private readonly IRecordService _recordService;
        private readonly ILogger<RecordController> _logger;

        public RecordController(IRecordService recordService, ILogger<RecordController> logger)
        {
            _recordService = recordService;
            _logger = logger;
        }

        [HttpPost("record")]
        [ProducesResponseType(typeof(OkSituation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddRecord([FromBody] AddRecordRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            var userId = ServiceMiddleware.GetUserId(HttpContext);
            _logger.LogInformation("Received request to add record for user {UserId}", userId);

            try
            {
                await _recordService.AddRecordAsync(userId, req);
                _logger.LogInformation("Create record Successfully");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ClientErrorSituation
                {
                    RequestId = requestId,
                    ErrorMessage = ex.Message
                });
            }

            return Ok(new OkSituation
            {
                RequestId = requestId
            });
        }

        [HttpGet("expenses")]
        [ProducesResponseType(typeof(GetThisMonthExpensesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetThisMonthExpenses()
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            var userId = ServiceMiddleware.GetUserId(HttpContext);
            _logger.LogInformation("Received request to get records for user {UserId}", userId);

            var expenses = await _recordService.GetThisMonthExpensesAsync(userId);

            return Ok(new GetThisMonthExpensesResponse
            {
                RequestId = requestId,
                Expenses = expenses
            });
        }
    }
}
