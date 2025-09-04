using Microsoft.AspNetCore.Mvc;
using SmartFlowBackend.Domain.Interfaces;
using Middleware;
using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain;

namespace SmartFlowBackend.Application.Controller
{
    [ApiController]
    [Route("smartflow/v1/record")]
    public class RecordController : ControllerBase
    {
        private readonly IRecordService _recordService;
        private readonly ILogger<RecordController> _logger;

        public RecordController(IRecordService recordService, ILogger<RecordController> logger)
        {
            _recordService = recordService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddRecord([FromBody] AddRecordRequest request)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to add record for user {UserId}", userId);

            try
            {
                await _recordService.AddRecordAsync(request, userId);
                _logger.LogInformation("Create record Successfully");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    requestId,
                    errorMessage = ex.Message
                });
            }

            return Ok(new
            {
                requestId
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetRecords([FromQuery] string period)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to get records for user {UserId} with period {Period}", userId, period);

            if (period == "this-month")
            {
                try
                {
                    var response = await _recordService.GetThisMonthRecordsAsync(userId);
                    return Ok(new
                    {
                        requestId,
                        response
                    });
                }
                catch (ArgumentException ex)
                {
                    return NotFound(new { requestId, errorMessage = ex.Message });
                }
            }
            else if (period == "last-six-months")
            {
                try
                {
                    var response = await _recordService.GetLastSixMonthRecordsAsync(userId);
                    return Ok(new
                    {
                        requestId,
                        response
                    });
                }
                catch (ArgumentException ex)
                {
                    return NotFound(new { requestId, errorMessage = ex.Message });
                }
            }
            else if (period == "all-months")
            {
                try
                {
                    var response = await _recordService.GetAllMonthRecordsAsync(userId);
                    return Ok(new
                    {
                        requestId,
                        response
                    });
                }
                catch (ArgumentException ex)
                {
                    return NotFound(new { requestId, errorMessage = ex.Message });
                }
            }

            return BadRequest(new
            {
                requestId,
                errorMessage = "Invalid period specified. Allowed values are 'this-month', 'last-six-months', 'all-months'."
            });
        }
    }
}
