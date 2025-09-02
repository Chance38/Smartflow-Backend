using Microsoft.AspNetCore.Mvc;
using SmartFlowBackend.Application.Contracts;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;
using Middleware;

namespace SmartFlowBackend.Controller
{
    [ApiController]
    [Route("smartflow/v1/record")]
    public class RecordController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RecordController> _logger;

        public RecordController(IUnitOfWork unitOfWork, ILogger<RecordController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> AddRecord([FromRoute] Guid userId, [FromBody] AddRecordRequest request)
        {
            _logger.LogInformation("Received request to add record for user {UserId}", userId);
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            try
            {
                var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("User not found");
                }

                var category = await _unitOfWork.Categories.GetCategoryByNameAsync(request.Category);
                if (category == null)
                {
                    throw new ArgumentException("Category not found");
                }

                var tag = await _unitOfWork.Tags.GetTagByNameAsync(request.Tag);
                if (tag == null)
                {
                    throw new ArgumentException("Tag not found");
                }

                var record = new Record
                {
                    Id = Guid.NewGuid(),
                    Type = request.Type,
                    Amount = request.Amount,
                    Date = request.Date,
                    UserId = userId,
                    CategoryId = category.Id,
                    TagId = tag.Id
                };
                await _unitOfWork.Records.AddAsync(record);
                await _unitOfWork.SaveAsync();
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

        [HttpGet("this-month/{userId}")]
        public async Task<ActionResult<GetThisMonthRecordResponse>> GetThisMonthRecords([FromRoute] Guid userId)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var balanceView = await _unitOfWork.Records.GetBalanceViewAsync(userId);
            var balance = balanceView?.Balance ?? 0;
            var records = await _unitOfWork.Records.GetRecordsByUserIdAndMonthAsync(userId, DateTime.Now.Year, DateTime.Now.Month);
            var totalExpense = records.Where(r => r.Type == CategoryType.Expense).Sum(r => r.Amount);
            var totalIncome = records.Where(r => r.Type == CategoryType.Income).Sum(r => r.Amount);
            var expenses = records
                .Where(r => r.Type == CategoryType.Expense)
                .GroupBy(r => r.Category.Name)
                .Select(g => new Expense
                {
                    Type = g.Key,
                    Amount = g.Sum(r => r.Amount)
                })
                .ToList();
            var response = new GetThisMonthRecordResponse
            {
                Balance = balance,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Expenses = expenses
            };
            return Ok(new
            {
                requestId,
                response
            });
        }

        [HttpGet("all-months/{userId}")]
        public async Task<ActionResult<IEnumerable<GetAllMonthRecordsResponse>>> GetAllMonthRecords([FromRoute] Guid userId)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var monthlyViews = await _unitOfWork.Records.GetMonthlyRecordsViewAsync(userId);
            var groupedRecords = monthlyViews
                .OrderBy(mv => mv.Year).ThenBy(mv => mv.Month)
                .Select(mv => new RecordPerMonth
                {
                    Year = mv.Year.ToString(),
                    Month = mv.Month.ToString(),
                    Expense = mv.Expense,
                    Income = mv.Income
                })
                .ToList();

            var response = new GetAllMonthRecordsResponse
            {
                Records = groupedRecords
            };

            return Ok(new
            {
                requestId,
                response
            });
        }
    }
}
