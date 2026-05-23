using System.Text;
using System.Text.Json;
using DriverService.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DriverService.Services;

public class DriverRpcConsumer(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<DriverRpcConsumer> logger) : BackgroundService
{
    private const string GetAvailableQueue = "driver.get-available.rpc";
    private const string AssignOrderQueue = "driver.assign-order.rpc";

    private readonly ConnectionFactory _factory = new()
    {
        HostName = configuration["RabbitMq:Host"] ?? "localhost",
        UserName = configuration["RabbitMq:User"] ?? "guest",
        Password = configuration["RabbitMq:Password"] ?? "guest",
        DispatchConsumersAsync = true
    };

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = _factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(GetAvailableQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueDeclare(AssignOrderQueue, durable: true, exclusive: false, autoDelete: false);
        channel.BasicQos(0, 1, false);

        var getAvailableConsumer = new AsyncEventingBasicConsumer(channel);
        getAvailableConsumer.Received += async (_, ea) =>
        {
            var response = HandleGetAvailable();
            Reply(channel, ea, response);
            channel.BasicAck(ea.DeliveryTag, false);
            await Task.CompletedTask;
        };

        var assignOrderConsumer = new AsyncEventingBasicConsumer(channel);
        assignOrderConsumer.Received += async (_, ea) =>
        {
            try
            {
                var response = HandleAssignOrder(ea.Body.ToArray());
                Reply(channel, ea, response);
                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle assign-order RPC");
                channel.BasicNack(ea.DeliveryTag, false, true);
            }
            await Task.CompletedTask;
        };

        channel.BasicConsume(GetAvailableQueue, autoAck: false, getAvailableConsumer);
        channel.BasicConsume(AssignOrderQueue, autoAck: false, assignOrderConsumer);

        stoppingToken.Register(() => { channel.Dispose(); connection.Dispose(); });
        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private string HandleGetAvailable()
    {
        using var scope = serviceProvider.CreateScope();
        var driversService = scope.ServiceProvider.GetRequiredService<DriversService>();
        var driver = driversService.GetAvailableAsync().GetAwaiter().GetResult();
        var response = driver is null
            ? new GetAvailableDriverRpcResponse(false, null)
            : new GetAvailableDriverRpcResponse(true, new DriverSummary(driver.Id, driver.Name, driver.Status));
        return JsonSerializer.Serialize(response);
    }

    private string HandleAssignOrder(byte[] body)
    {
        var request = JsonSerializer.Deserialize<AssignOrderRpcRequest>(Encoding.UTF8.GetString(body))
                      ?? throw new InvalidOperationException("Invalid request body");

        using var scope = serviceProvider.CreateScope();
        var driversService = scope.ServiceProvider.GetRequiredService<DriversService>();
        var success = driversService.AssignOrderAsync(request.DriverId, request.OrderId).GetAwaiter().GetResult();
        var driver = driversService.GetByIdAsync(request.DriverId).GetAwaiter().GetResult();

        return JsonSerializer.Serialize(new AssignOrderRpcResponse(success, driver?.Name, request.ClientId));
    }

    private static void Reply(IModel channel, BasicDeliverEventArgs ea, string response)
    {
        var replyProps = channel.CreateBasicProperties();
        replyProps.CorrelationId = ea.BasicProperties.CorrelationId;
        var responseBytes = Encoding.UTF8.GetBytes(response);
        channel.BasicPublish(string.Empty, ea.BasicProperties.ReplyTo, replyProps, responseBytes);
    }
}
