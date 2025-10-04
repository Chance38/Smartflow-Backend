namespace Domain.Interface;

public interface IIdentityHostService
{
    Task StartConsumingAsync(string queueName, string exchangeName, string routingKey);
}
