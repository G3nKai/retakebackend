using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using OrderService.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderService.Services;

public class DriverGatewayClient : IDisposable
{
    private const string GetAvailableQueue = "driver.get-available.rpc";
    private const string AssignOrderQueue = "driver.assign-order.rpc";
    private const string NotificationQueue = "notifications.order-created";

    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _replyQueueName;
    private readonly EventingBasicConsumer _consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pending = new();

    public DriverGatewayClient(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
            UserName = configuration["RabbitMq:User"] ?? "guest",
            Password = configuration["RabbitMq:Password"] ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(GetAvailableQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare(AssignOrderQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare(NotificationQueue, durable: false, exclusive: false, autoDelete: false);

        var replyQueue = _channel.QueueDeclare(queue: string.Empty, durable: false, exclusive: true, autoDelete: true);
        _replyQueueName = replyQueue.QueueName;

        _consumer = new EventingBasicConsumer(_channel);
        _consumer.Received += (_, ea) =>
        {
            var correlationId = ea.BasicProperties.CorrelationId;
            if (correlationId is null || !_pending.TryRemove(correlationId, out var tcs)) return;
            tcs.TrySetResult(Encoding.UTF8.GetString(ea.Body.ToArray()));
        };

        _channel.BasicConsume(_replyQueueName, autoAck: true, _consumer);
    }

    public async Task<DriverSummary?> GetAvailableDriverAsync(CancellationToken cancellationToken = default)
    {
        var response = await CallAsync<GetAvailableDriverRpcResponse>(GetAvailableQueue, new GetAvailableDriverRpcRequest(), cancellationToken);
        return response.Found ? response.Driver : null;
    }

    public async Task<AssignOrderRpcResponse> AssignOrderToDriverAsync(Guid driverId, Guid orderId, Guid clientId, CancellationToken cancellationToken = default)
        => await CallAsync<AssignOrderRpcResponse>(AssignOrderQueue, new AssignOrderRpcRequest(driverId, orderId, clientId), cancellationToken);

    public void PublishOrderAssignedEvent(OrderAssignedNotificationEvent notification)
    {
        var props = _channel.CreateBasicProperties();
        props.MessageId = $"{notification.OrderId}:{notification.DriverId}";
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(notification));
        _channel.BasicPublish(string.Empty, NotificationQueue, props, body);
    }

    private async Task<TResponse> CallAsync<TResponse>(string queue, object request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[correlationId] = tcs;

        var props = _channel.CreateBasicProperties();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        props.Persistent = true;

        _channel.BasicPublish(string.Empty, queue, props, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)));

        using var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
        var json = await tcs.Task;
        return JsonSerializer.Deserialize<TResponse>(json) ?? throw new InvalidOperationException("Invalid RPC response");
    }

    public void Dispose() { _channel.Dispose(); _connection.Dispose(); }
}
