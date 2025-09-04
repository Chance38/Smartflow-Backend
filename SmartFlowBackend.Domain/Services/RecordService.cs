using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Domain.Services
{
    public class RecordService : IRecordService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecordService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddRecordAsync(AddRecordRequest request, Guid userId)
        {
            var user = await _unitOfWork.User.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var summary = await _unitOfWork.MonthlySummary.FindAsync(s => s.UserId == userId && s.Year == request.Date.Year && s.Month == request.Date.Month);

            if (request.Type == CategoryType.Expense)
            {
                user.Balance -= request.Amount;
                if (summary == null)
                {
                    summary = new MonthlySummary
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Year = request.Date.Year,
                        Month = request.Date.Month,
                        Expense = request.Amount,
                        Income = 0
                    };
                    await _unitOfWork.MonthlySummary.AddAsync(summary);
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
                    summary = new MonthlySummary
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Year = request.Date.Year,
                        Month = request.Date.Month,
                        Income = request.Amount,
                        Expense = 0
                    };
                    await _unitOfWork.MonthlySummary.AddAsync(summary);
                }
                else
                {
                    summary.Income += request.Amount;
                }
            }

            var category = await _unitOfWork.Category.GetCategoryByNameAsync(request.Category);
            if (category == null)
            {
                throw new ArgumentException("Category not found");
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

            if (request.Tag != null)
            {
                var tag = await _unitOfWork.Tag.GetTagByNameAsync(request.Tag);
                if (tag == null)
                {
                    throw new ArgumentException("Tag not found");
                }

                record.Tags.Add(tag);
            }

            await _unitOfWork.Record.AddAsync(record);
            await _unitOfWork.SaveAsync();
        }

        public async Task<GetThisMonthRecordResponse> GetThisMonthRecordsAsync(Guid userId)
        {
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var records = await _unitOfWork.Record.GetRecordsByUserIdAndMonthAsync(userId, DateTime.Now.Year, DateTime.Now.Month);
            var summary = await _unitOfWork.MonthlySummary.FindAsync(s => s.UserId == userId && s.Year == DateTime.Now.Year && s.Month == DateTime.Now.Month);

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

            return new GetThisMonthRecordResponse
            {
                Balance = user.Balance,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Expenses = expenses
            };
        }

        public async Task<GetLastSixMonthRecordsResponse> GetLastSixMonthRecordsAsync(Guid userId)
        {
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var start = DateTime.Now.AddMonths(-5);
            var monthlySummaries = await _unitOfWork.MonthlySummary.FindAllAsync(
                s => s.UserId == userId && (s.Year > start.Year || (s.Year == start.Year && s.Month >= start.Month)));

            var dict = monthlySummaries.ToDictionary(s => (s.Year, s.Month));

            var records = new List<RecordPerMonth>(6);
            for (int i = 0; i < 6; i++)
            {
                var dt = start.AddMonths(i);
                if (dict.TryGetValue((dt.Year, dt.Month), out var summary))
                {
                    records.Add(new RecordPerMonth
                    {
                        Year = summary.Year,
                        Month = summary.Month,
                        Expense = summary.Expense,
                        Income = summary.Income
                    });
                }
                else
                {
                    records.Add(new RecordPerMonth
                    {
                        Year = dt.Year,
                        Month = dt.Month,
                        Expense = 0,
                        Income = 0
                    });
                }
            }

            return new GetLastSixMonthRecordsResponse
            {
                Records = records
            };
        }

        public async Task<GetAllMonthRecordsResponse> GetAllMonthRecordsAsync(Guid userId)
        {
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var monthlySummaries = await _unitOfWork.MonthlySummary.FindAllAsync(s => s.UserId == userId);
            var groupedRecords = monthlySummaries
                .OrderBy(s => s.Year).ThenBy(s => s.Month)
                .Select(s => new RecordPerMonth
                {
                    Year = s.Year,
                    Month = s.Month,
                    Expense = s.Expense,
                    Income = s.Income
                })
                .ToList();

            return new GetAllMonthRecordsResponse
            {
                Records = groupedRecords
            };
        }
    }
}
