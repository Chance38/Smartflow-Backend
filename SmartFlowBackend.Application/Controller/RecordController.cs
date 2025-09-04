using Microsoft.AspNetCore.Mvc;
using SmartFlowBackend.Domain.Interfaces;
using Middleware;
using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain;
using Swashbuckle.AspNetCore.Filters;

namespace SmartFlowBackend.Application.Controller
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
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddRecord([FromBody] AddRecordRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to add record for user {UserId}", userId);

            try
            {
                await _recordService.AddRecordAsync(req, userId);
                _logger.LogInformation("Create record Successfully");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ClientErrorSituation
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
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetThisMonthExpenses()
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to get records for user {UserId}", userId);

            try
            {
                var expenses = await _recordService.GetThisMonthExpensesAsync(userId);
                return Ok(new GetThisMonthExpensesResponse
                {
                    RequestId = requestId,
                    Expenses = expenses
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ClientErrorSituation
                {
                    RequestId = requestId,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpGet("month-records")]
        [ProducesResponseType(typeof(GetMonthRecordsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMonthRecords([FromQuery] int period)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to get records for user {UserId} with period {Period}", userId, period);
            if (period == 1)
            {
                try
                {
                    var response = await _recordService.GetThisMonthRecordsAsync(userId);
                    return Ok(new GetMonthRecordsResponse
                    {
                        RequestId = requestId,
                        Records = response
                    });
                }
                catch (ArgumentException ex)
                {
                    return NotFound(new ClientErrorSituation
                    {
                        RequestId = requestId,
                        ErrorMessage = ex.Message
                    });
                }
            }
            else if (period == 6)
            {
                try
                {
                    var response = await _recordService.GetLastSixMonthRecordsAsync(userId);
                    return Ok(new GetMonthRecordsResponse
                    {
                        RequestId = requestId,
                        Records = response
                    });
                }
                catch (ArgumentException ex)
                {
                    return NotFound(new ClientErrorSituation
                    {
                        RequestId = requestId,
                        ErrorMessage = ex.Message
                    });
                }
            }
            else
            {
                try
                {
                    var response = await _recordService.GetAllMonthRecordsAsync(userId);
                    return Ok(new GetMonthRecordsResponse
                    {
                        RequestId = requestId,
                        Records = response
                    });
                }
                catch (ArgumentException ex)
                {
                    return NotFound(new ClientErrorSituation
                    {
                        RequestId = requestId,
                        ErrorMessage = ex.Message
                    });
                }
            }
        }
    }
}
