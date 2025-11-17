using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Domain.Interface;

namespace Infrastructure.Messaging;

public class IdentityHostService : IIdentityHostService
{
    private readonly ILogger<IdentityHostService> _logger;
    private readonly RabbitMqConnection _connection;
    private readonly IServiceProvider _serviceProvider;

    public IdentityHostService(
        RabbitMqConnection connection,
        ILogger<IdentityHostService> logger,
        IServiceProvider serviceProvider)
    {
        _connection = connection;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartConsumingAsync(string queueName, string exchangeName, string routingKey)
    {
        var channel = _connection.Channel;

        if (channel == null || channel.IsClosed)
        {
            const string errorMessage = "RabbitMQ channel is not available, cannot start consuming.";
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        try
        {
            _logger.LogInformation("Declaring topic exchange '{ExchangeName}'...", exchangeName);
            channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true, autoDelete: false);

            _logger.LogInformation("Declaring queue '{QueueName}'...", queueName);
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            _logger.LogInformation("Binding queue '{QueueName}' to exchange '{ExchangeName}' with routing key '{RoutingKey}'...", queueName, exchangeName, routingKey);
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

            _logger.LogInformation("Successfully bound queue '{QueueName}' to exchange '{ExchangeName}'.", queueName, exchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bind RabbitMQ queue '{QueueName}' to exchange '{ExchangeName}'.", queueName, exchangeName);
            throw;
        }

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received message: {Message}", message);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userRegisterSubscriber = scope.ServiceProvider.GetRequiredService<IUserRegisterSubscriber>();
                    await userRegisterSubscriber.HandleAsync(message);
                }

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", message);
                channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        channel.BasicConsume(queue: queueName,
                             autoAck: false,
                             consumer: consumer);

        _logger.LogInformation("Started consuming from queue '{QueueName}'.", queueName);
        return Task.CompletedTask;
    }
}