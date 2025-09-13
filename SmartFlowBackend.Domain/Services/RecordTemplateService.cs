using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Domain.Services
{
    public class RecordTemplateService : IRecordTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecordTemplateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddRecordTemplateAsync(AddRecordTemplateRequest req, Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var existingTemplate = await _unitOfWork.RecordTemplate.FindAsync(rt => rt.UserId == userId && rt.RecordTemplateName == req.RecordTemplate.RecordTemplateName);
            if (existingTemplate != null)
            {
                throw new ArgumentException("Record template with the same name already exists.");
            }

            if (req.RecordTemplate.Amount == 0 &&
                string.IsNullOrEmpty(req.RecordTemplate.CategoryName) &&
                (req.RecordTemplate.Tags == null || !req.RecordTemplate.Tags.Any()))
            {
                throw new ArgumentException("At least one field must be filled");
            }

            var tags = req.RecordTemplate.Tags.Distinct().ToList();

            var recordTemplate = new Domain.Entities.RecordTemplate
            {
                RecordTemplateId = Guid.NewGuid(),
                RecordTemplateName = req.RecordTemplate.RecordTemplateName,
                CategoryName = req.RecordTemplate.CategoryName,
                CategoryType = req.RecordTemplate.CategoryType,
                TagNames = tags,
                Amount = req.RecordTemplate.Amount,
                UserId = userId
            };

            await _unitOfWork.RecordTemplate.AddAsync(recordTemplate);
            await _unitOfWork.SaveAsync();
        }

        public async Task<List<Domain.Contracts.RecordTemplate>> GetAllRecordTemplatesAsync(Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var recordTemplates = await _unitOfWork.RecordTemplate.FindAllAsync(rt => rt.UserId == userId);

            return recordTemplates.Select(rt => new Domain.Contracts.RecordTemplate
            {
                RecordTemplateName = rt.RecordTemplateName,
                CategoryName = rt.CategoryName,
                CategoryType = rt.CategoryType,
                Tags = rt.TagNames,
                Amount = rt.Amount
            }).ToList();
        }

        public async Task DeleteRecordTemplateAsync(DeleteRecordTemplateRequest req, Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                return;
            }

            var recordTemplate = await _unitOfWork.RecordTemplate.FindAsync(rt => rt.UserId == userId && rt.RecordTemplateName == req.RecordTemplateName);
            if (recordTemplate == null)
            {
                return;
            }

            await _unitOfWork.RecordTemplate.DeleteAsync(recordTemplate.RecordTemplateId);
            await _unitOfWork.SaveAsync();
        }
    }
}
