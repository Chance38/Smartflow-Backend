using Contracts.Record;
using Microsoft.AspNetCore.Mvc;
using Middleware;

namespace Controller.Record;

[ApiController]
[Route("smartflow/v1/record")]
public class RecordController : ControllerBase
{
    public RecordController()
    {
    }

    [HttpPost("{userId}")]
    public async Task<IActionResult> AddRecord([FromRoute(Name = "userId")] string userId, [FromBody] AddRecordRequest req)
    {
        var requestId = ServiceMiddleware.GetRequestId(HttpContext);

        return Ok(new
        {
            RequestId = requestId
        });
    }

    [HttpGet("month/{userId}")]
    public async Task<IActionResult> GetThisMonthRecord([FromRoute(Name = "userId")] string userId)
    {
        var requestId = ServiceMiddleware.GetRequestId(HttpContext);

        return Ok(new
        {
            RequestId = requestId,
            data = new GetThisMonthRecordResponse
            {
                Balance = "1000",
                TotalExpense = 2500,
                TotalIncome = 1500,
                Expenses = new List<Expense>
                {
                    new Expense { Type = "food", Amount = 1000 },
                    new Expense { Type = "3C Product", Amount = 500 }
                }
            }
        });
    }

    [HttpGet("months/{userId}")]
    public async Task<IActionResult> GetAllMonthRecords([FromRoute(Name = "userId")] string userId)
    {
        var requestId = ServiceMiddleware.GetRequestId(HttpContext);

        return Ok(new
        {
            RequestId = requestId,
            data = new GetAllMonthRecordsResponse
            {
                Year = "2025",
                Month = "8",
                Expense = 2500,
                Income = 1500
            }
        });
    }
}