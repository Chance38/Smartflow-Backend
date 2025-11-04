using Microsoft.AspNetCore.Mvc;
using Middleware;
using Application.Contract;
using Application.Interface.Service;

namespace Presentation.Controller
{
    [ApiController]
    [Route("smartflow/v1")]
    public class SummaryController : ControllerBase
    {
        private readonly ISummaryService _summaryService;
        private readonly ILogger<SummaryController> _logger;

        public SummaryController(ISummaryService summaryService, ILogger<SummaryController> logger)
        {
            _summaryService = summaryService;
            _logger = logger;
        }

        [HttpGet("month-summary")]
        [ProducesResponseType(typeof(GetMonthSummariesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMonthSummaries([FromQuery] int? period)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            var userId = ServiceMiddleware.GetUserId(HttpContext);
            _logger.LogInformation("Received request to get records for user {UserId} with period {Period}", userId, period);

            if (!period.HasValue)
            {
                var records = await _summaryService.GetAllSummariesAsync(userId);
                return Ok(new GetMonthSummariesResponse
                {
                    RequestId = requestId,
                    Summaries = records
                });
            }

            switch (period.Value)
            {
                case 1:
                    {
                        var now = DateTime.UtcNow;
                        var records = await _summaryService.GetSummariesAsync(
                            userId,
                            now.Year, now.Month,
                            now.Year, now.Month
                        );
                        return Ok(new GetMonthSummariesResponse
                        {
                            RequestId = requestId,
                            Summaries = records
                        });
                    }
                case 6:
                    {
                        var end = DateTime.UtcNow;
                        var start = end.AddMonths(-5);

                        var records = await _summaryService.GetSummariesAsync(
                            userId,
                            start.Year, start.Month,
                            end.Year, end.Month
                        );
                        return Ok(new GetMonthSummariesResponse
                        {
                            RequestId = requestId,
                            Summaries = records
                        });
                    }
                default:
                    return BadRequest(new ClientErrorSituation
                    {
                        RequestId = requestId,
                        ErrorMessage = "Invalid period. Supported values are 1 (this month) and 6 (last six months)."
                    });
            }
        }
    }
}
