using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;

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
            var user = await _unitOfWork.User.FindAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            return user.Balance;
        }
    }
}
