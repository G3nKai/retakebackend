using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<NotificationConsumer>();
var app = builder.Build();
app.MapGet("/health", () => Results.Ok("ok"));
app.Run();

public record OrderAssignedNotificationEvent(Guid OrderId, Guid DriverId, string DriverName, Guid ClientId);

public class NotificationConsumer(IConfiguration configuration, ILogger<NotificationConsumer> logger) : BackgroundService
{
    private const string QueueName = "notifications.order-created";

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
            UserName = configuration["RabbitMq:User"] ?? "guest",
            Password = configuration["RabbitMq:Password"] ?? "guest",
            DispatchConsumersAsync = true
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        channel.QueueDeclare(QueueName, durable: false, exclusive: false, autoDelete: false);
        channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var notification = JsonSerializer.Deserialize<OrderAssignedNotificationEvent>(Encoding.UTF8.GetString(ea.Body.ToArray()));
                if (notification is not null)
                {
                    logger.LogInformation("Пользователю {ClientId}: к вам едет водитель {DriverName}. Заказ {OrderId}",
                        notification.ClientId, notification.DriverName, notification.OrderId);
                }

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch
            {
                channel.BasicNack(ea.DeliveryTag, false, false);
            }

            await Task.CompletedTask;
        };

        channel.BasicConsume(QueueName, autoAck: false, consumer);
        stoppingToken.Register(() => { channel.Dispose(); connection.Dispose(); });
        return Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
