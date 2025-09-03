using Microsoft.AspNetCore.Mvc;
using SmartFlowBackend.Application;
using SmartFlowBackend.Application.Contracts;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;
using Middleware;

namespace SmartFlowBackend.Controller
{
    [ApiController]
    [Route("smartflow/v1/records")]
    public class RecordController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RecordController> _logger;

        public RecordController(IUnitOfWork unitOfWork, ILogger<RecordController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddRecord([FromBody] AddRecordRequest request)
        {
            var userId = TestUser.Id;
            _logger.LogInformation("Received request to add record for user {UserId}", userId);
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            try
            {
                var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("User not found");
                }

                var summary = await _unitOfWork.MonthlySummaries.FindAsync(s => s.UserId == userId && s.Year == request.Date.Year && s.Month == request.Date.Month);

                if (request.Type == CategoryType.Expense)
                {
                    user.Balance -= request.Amount;
                    if (summary == null)
                    {
                        summary = new MonthlySummary { Id = Guid.NewGuid(), UserId = userId, Year = request.Date.Year, Month = request.Date.Month, Expense = request.Amount, Income = 0 };
                        await _unitOfWork.MonthlySummaries.AddAsync(summary);
                    }
                    else
                    {
                        summary.Expense += request.Amount;
                    }
                }
                else
                {
                    user.Balance += request.Amount;
                    if (summary == null)
                    {
                        summary = new MonthlySummary { Id = Guid.NewGuid(), UserId = userId, Year = request.Date.Year, Month = request.Date.Month, Income = request.Amount, Expense = 0 };
                        await _unitOfWork.MonthlySummaries.AddAsync(summary);
                    }
                    else
                    {
                        summary.Income += request.Amount;
                    }
                }

                var category = await _unitOfWork.Categories.GetCategoryByNameAsync(request.Category);
                if (category == null)
                {
                    throw new ArgumentException("Category not found");
                }

                if (string.IsNullOrEmpty(request.Tag))
                {
                    throw new ArgumentException("Tag cannot be null or empty");
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
                };
                record.Tags.Add(tag);
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

        [HttpGet]
        public async Task<IActionResult> GetRecords([FromQuery] string period)
        {
            if (period == "this-month")
            {
                return await GetThisMonthRecords();
            }
            else if (period == "all-months")
            {
                return await GetAllMonthRecords();
            }
            else
            {
                return BadRequest("Invalid period specified. Allowed values are 'this-month' or 'all-months'.");
            }
        }

        private async Task<IActionResult> GetThisMonthRecords()
        {
            var userId = TestUser.Id;
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { requestId, errorMessage = "User not found" });
            }

            var records = await _unitOfWork.Records.GetRecordsByUserIdAndMonthAsync(userId, DateTime.Now.Year, DateTime.Now.Month);
            var summary = await _unitOfWork.MonthlySummaries.FindAsync(s => s.UserId == userId && s.Year == DateTime.Now.Year && s.Month == DateTime.Now.Month);

            var totalExpense = summary?.Expense ?? 0;
            var totalIncome = summary?.Income ?? 0;
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
                Balance = user.Balance,
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

        private async Task<IActionResult> GetAllMonthRecords()
        {
            var userId = TestUser.Id;
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var monthlySummaries = await _unitOfWork.MonthlySummaries.FindAllAsync(s => s.UserId == userId);
            var groupedRecords = monthlySummaries
                .OrderBy(s => s.Year).ThenBy(s => s.Month)
                .Select(s => new RecordPerMonth
                {
                    Year = s.Year.ToString(),
                    Month = s.Month.ToString(),
                    Expense = s.Expense,
                    Income = s.Income
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
