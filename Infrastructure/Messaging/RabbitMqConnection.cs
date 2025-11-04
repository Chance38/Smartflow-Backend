using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using System;

namespace Infrastructure.Messaging;

public class RabbitMqConnection : IDisposable
{
    private readonly ILogger<RabbitMqConnection> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly string _hostName;
    private readonly int _port;
    private readonly string _userName;
    private readonly string _password;

    public IModel? Channel => _channel;

    public RabbitMqConnection(ILogger<RabbitMqConnection> logger)
    {
        _logger = logger;
        _hostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq.host.is.not.given";
        _port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var port) ? port : 5672;
        _userName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "rabbitmq.user.is.not.given";
        _password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "rabbitmq.password.is.not.given";
        Connect();
    }

    private void Connect()
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            Port = _port,
            UserName = _userName,
            Password = _password,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _logger.LogInformation("RabbitMQ connection established.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set up RabbitMQ connection.");
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            _logger.LogInformation("RabbitMQ resources disposed.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "An error occurred while disposing RabbitMQ resources.");
        }
    }
}
