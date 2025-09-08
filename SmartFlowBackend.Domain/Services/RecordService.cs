using Microsoft.EntityFrameworkCore;
using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;
using System.Linq;

namespace SmartFlowBackend.Domain.Services
{
    public class RecordService : IRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISummaryService _summaryService;
        private readonly IBalanceService _balanceService;

        public RecordService(
            IUnitOfWork unitOfWork,
            ISummaryService summaryService,
            IBalanceService balanceService)
        {
            _unitOfWork = unitOfWork;
            _summaryService = summaryService;
            _balanceService = balanceService;
        }

        public async Task AddRecordAsync(AddRecordRequest request, Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var category = await _unitOfWork.Category.FindAsync(c => c.CategoryName == request.Category && c.Type == request.Type);
            if (category == null)
            {
                throw new ArgumentException("Category not found");
            }

            var record = new Record
            {
                RecordId = Guid.NewGuid(),
                CategoryId = category.CategoryId,
                CategoryName = request.Category,
                Type = category.Type,
                Amount = request.Amount,
                Date = request.Date,
                UserId = userId
            };

            if (request.Tag != null && request.Tag.Any())
            {
                var tags = await _unitOfWork.Tag.FindAllAsync(t => request.Tag.Contains(t.TagName) && t.UserId == userId);
                if (tags == null || tags.Count() != request.Tag.Count)
                {
                    throw new ArgumentException("One or more tags not found");
                }

                foreach (var tag in tags)
                {
                    record.Tags.Add(tag);
                }

                record.TagNames.AddRange(tags.Select(t => t.TagName));
            }
            await _unitOfWork.Record.AddAsync(record);

            await _summaryService.UpdateMonthlySummaryAsync(user, category.Type, request.Date.Year, request.Date.Month, request.Amount);
            await _balanceService.UpdateBalanceAsync(user, category.Type, request.Amount);

            await _unitOfWork.SaveAsync();
        }

        public async Task<List<Expense>> GetThisMonthExpensesAsync(Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var records = await _unitOfWork.Record.FindAllAsync(r => r.UserId == userId && r.Date.Year == DateTime.Now.Year && r.Date.Month == DateTime.Now.Month);

            var expenses = records
                .Where(r => r.Type == CategoryType.Expense)
                .GroupBy(r => r.CategoryName)
                .Select(g => new Expense
                {
                    Type = g.Key,
                    Amount = g.Sum(r => r.Amount)
                }).ToList();

            return expenses;
        }

        public async Task<List<RecordPerMonth>> GetThisMonthRecordsAsync(Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var summary = await _unitOfWork.MonthlySummary.FindAsync(s => s.UserId == userId && s.Year == DateTime.Now.Year && s.Month == DateTime.Now.Month);

            if (summary == null)
            {
                return new List<RecordPerMonth>();
            }

            var record = new RecordPerMonth
            {
                Year = summary.Year,
                Month = summary.Month,
                Expense = summary.Expense,
                Income = summary.Income
            };

            return new List<RecordPerMonth> { record };
        }

        public async Task<List<RecordPerMonth>> GetLastSixMonthRecordsAsync(Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
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

            return records;
        }

        public async Task<List<RecordPerMonth>> GetAllMonthRecordsAsync(Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
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

            return groupedRecords;
        }
    }
}
