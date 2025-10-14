using Domain.Interface;

namespace Domain.Service;

public class RecordTemplateService : IRecordTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRecordTemplateRepository _repo;

    public RecordTemplateService(IUnitOfWork unitOfWork, IRecordTemplateRepository repo)
    {
        _unitOfWork = unitOfWork;
        _repo = repo;
    }

    public async Task AddRecordTemplateAsync(Guid userId, Contract.RecordTemplate temp)
    {
        var existTemp = await _repo.CheckExistAsync(userId, temp.Name);
        if (existTemp != null)
        {
            throw new ArgumentException("Category already exists");
        }

        if (string.IsNullOrEmpty(temp.CategoryName) &&
            (temp.Tags == null || !temp.Tags.Any()))
        {
            throw new ArgumentException("At least one field must be filled");
        }

        var typeEntity = temp.CategoryType switch
        {
            Contract.CategoryType.EXPENSE => Entity.CategoryType.EXPENSE,
            Contract.CategoryType.INCOME => Entity.CategoryType.INCOME,
            _ => throw new ArgumentException("Invalid category type")
        };

        var tagNames = temp.Tags
            .Select(t => t.Name)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();

        var recordTemplate = new Entity.RecordTemplate
        {
            Id = Guid.NewGuid(),
            Name = temp.Name,
            CategoryName = temp.CategoryName,
            CategoryType = typeEntity,
            TagNames = tagNames,
            Amount = temp.Amount,
            UserId = userId
        };

        await _repo.AddAsync(recordTemplate);
        await _unitOfWork.SaveAsync();
    }

    public async Task<List<Contract.RecordTemplate>> GetAllRecordTemplatesAsync(Guid userId)
    {
        var temps = await _repo.GetAllRecordTemplatesAsync(userId);
        return temps.Select(t => new Contract.RecordTemplate
        {
            Name = t.Name,
            CategoryName = t.CategoryName,
            CategoryType = t.CategoryType switch
            {
                Entity.CategoryType.EXPENSE => Contract.CategoryType.EXPENSE,
                Entity.CategoryType.INCOME => Contract.CategoryType.INCOME,
                _ => throw new ArgumentException("data has unexpected category type, this should not happen")
            },
            Tags = t.TagNames?
                .Select(tagName => new Contract.Tag { Name = tagName })
                .ToList() ?? new List<Contract.Tag>(),
            Amount = t.Amount
        }).ToList();
    }

    public async Task DeleteRecordTemplateAsync(Guid userId, string name)
    {
        try
        {
            await _repo.DeleteAsync(userId, name);
        }
        catch (ArgumentException)
        {
            return;
        }
        await _unitOfWork.SaveAsync();
    }
}
