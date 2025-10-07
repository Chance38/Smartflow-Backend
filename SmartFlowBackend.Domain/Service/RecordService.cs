using Domain.Interface;

namespace Domain.Service;

public class RecordService : IRecordService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRecordRepository _recordRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly ITagRepository _tagRepo;

    public RecordService(
        IUnitOfWork unitOfWork,
        IRecordRepository recordRepo,
        ICategoryRepository categoryRepo,
        ITagRepository tagRepo)
    {
        _unitOfWork = unitOfWork;
        _recordRepo = recordRepo;
        _categoryRepo = categoryRepo;
        _tagRepo = tagRepo;
    }

    public async Task AddRecordAsync(Guid userId, Contract.AddRecordRequest req)
    {
        var typeEntity = req.Category.Type switch
        {
            Contract.CategoryType.EXPENSE => Entity.CategoryType.EXPENSE,
            Contract.CategoryType.INCOME => Entity.CategoryType.INCOME,
            _ => throw new ArgumentException("Invalid category type")
        };

        var existCategory = await _categoryRepo.CheckExistAsync(userId, req.Category.Name, typeEntity);
        if (existCategory == null)
        {
            throw new ArgumentException("Category does not exist");
        }

        var record = new Entity.Record
        {
            Id = Guid.NewGuid(),
            Amount = req.Amount,
            CategoryName = req.Category.Name,
            Type = typeEntity,
            Date = req.Date,
            UserId = userId
        };

        if (req.Tags != null && req.Tags.Any())
        {
            var existTags = await _tagRepo.CheckAllTagsExistAsync(userId, req.Tags.Select(t => t.Name).ToList());
            if (existTags.Count != req.Tags.Count)
            {
                throw new ArgumentException("There are some tags that do not exist");
            }

            record.TagNames = req.Tags
                .Select(t => t.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            record.Tags = existTags;
        }

        await _recordRepo.AddAsync(record);
        await _unitOfWork.SaveAsync();
    }

    public async Task<List<Contract.Expense>> GetThisMonthExpensesAsync(Guid userId)
    {
        var records = await _recordRepo.GetAllRecordsAsync(userId, DateTime.Now.Year, DateTime.Now.Month);

        var expenses = records
            .Where(r => r.Type == Entity.CategoryType.EXPENSE)
            .GroupBy(r => r.CategoryName)
            .Select(g => new Contract.Expense
            {
                CategoryName = g.Key,
                Amount = g.Sum(r => r.Amount)
            }).ToList();

        return expenses;
    }
}
