using Domain.Entity;
using Domain.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Domain.Subscriber;

public class UserRegisterSubscriber : BackgroundService, IUserRegisterSubscriber
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserRegisterSubscriber> _logger;
    private readonly IIdentityHostService _identityHostService;

    public UserRegisterSubscriber(IServiceProvider serviceProvider, ILogger<UserRegisterSubscriber> logger, IIdentityHostService identityHostService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _identityHostService = identityHostService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UserRegisterSubscriber background service started.");
        await _identityHostService.StartConsumingAsync("SMARTFLOW-BACKEND", "IDENTITY", "REGISTER");
    }

    public async Task HandleAsync(string message)
    {
        using var scope = _serviceProvider.CreateScope();
        var categoryRepo = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        var balanceRepo = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        try
        {
            var messageObject = JsonConvert.DeserializeObject<UserRegisterMessage>(message);
            if (messageObject is null || messageObject.UserId == Guid.Empty)
            {
                _logger.LogWarning("Invalid message format or empty user id.");
                return;
            }

            var userId = messageObject.UserId;

            await categoryRepo.AddRangeAsync(new List<Category>
            {
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "食物",
                    Type = CategoryType.EXPENSE,
                    UserId = userId
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "服飾",
                    Type = CategoryType.EXPENSE,
                    UserId = userId
                },
                new Category{
                    Id = Guid.NewGuid(),
                    Name = "交通",
                    Type = CategoryType.EXPENSE,
                    UserId = userId
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "娛樂",
                    Type = CategoryType.EXPENSE,
                    UserId = userId
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "薪水",
                    Type = CategoryType.INCOME,
                    UserId = userId
                }
            });

            var balance = new Balance
            {
                Id = Guid.NewGuid(),
                Amount = 0,
                UserId = userId
            };
            await balanceRepo.AddAsync(balance);
            await unitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling user register message");
            throw;
        }
    }
}

internal class UserRegisterMessage
{
    public Guid UserId { get; set; }
}
