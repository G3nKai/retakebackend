using System.Text;
using System.Text.Json;
using DriverService.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DriverService.Services;

public class DriverRpcConsumer(IServiceProvider serviceProvider, IConfiguration configuration) : BackgroundService
{
    private const string GetAvailableQueue = "driver.get-available.rpc";
    private const string AssignOrderQueue = "driver.assign-order.rpc";

    private readonly ConnectionFactory _factory = new()
    {
        HostName = configuration["RabbitMq:Host"] ?? "localhost",
        UserName = configuration["RabbitMq:User"] ?? "guest",
        Password = configuration["RabbitMq:Password"] ?? "guest"
    };

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = _factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(GetAvailableQueue, durable: false, exclusive: false, autoDelete: false);
        channel.QueueDeclare(AssignOrderQueue, durable: false, exclusive: false, autoDelete: false);

        var getAvailableConsumer = new EventingBasicConsumer(channel);
        getAvailableConsumer.Received += (_, ea) =>
        {
            var response = HandleGetAvailable();
            Reply(channel, ea, response);
        };

        var assignOrderConsumer = new EventingBasicConsumer(channel);
        assignOrderConsumer.Received += (_, ea) =>
        {
            var response = HandleAssignOrder(ea.Body.ToArray());
            Reply(channel, ea, response);
        };

        channel.BasicConsume(GetAvailableQueue, autoAck: true, getAvailableConsumer);
        channel.BasicConsume(AssignOrderQueue, autoAck: true, assignOrderConsumer);

        stoppingToken.Register(() =>
        {
            channel.Dispose();
            connection.Dispose();
        });

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

        return JsonSerializer.Serialize(new AssignOrderRpcResponse(success));
    }

    private static void Reply(IModel channel, BasicDeliverEventArgs ea, string response)
    {
        var replyProps = channel.CreateBasicProperties();
        replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

        var responseBytes = Encoding.UTF8.GetBytes(response);
        channel.BasicPublish(exchange: string.Empty, routingKey: ea.BasicProperties.ReplyTo, basicProperties: replyProps, body: responseBytes);
    }
}
