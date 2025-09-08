using SmartFlowBackend.Domain.Interfaces;
using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Services
{
    public class BalanceService : IBalanceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BalanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<float> GetBalanceByUserIdAsync(Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            return user.Balance;
        }

        public async Task UpdateBalanceAsync(User user, CategoryType type, float amount)
        {
            switch (type)
            {
                case CategoryType.Expense:
                    user.Balance -= amount;
                    break;

                case CategoryType.Income:
                    user.Balance += amount;
                    break;

                default:
                    throw new ArgumentException(nameof(type), "Invalid category type");
            }

            await _unitOfWork.User.UpdateAsync(user);
        }
    }
}
