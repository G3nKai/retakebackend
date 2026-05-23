using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using retakebackend.Messaging;

namespace retakebackend.Services;

public class RabbitPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitPublisher(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
            UserName = configuration["RabbitMq:Username"] ?? "guest",
            Password = configuration["RabbitMq:Password"] ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "todo-events", durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    public void Publish(TodoMessage message)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = _channel.CreateBasicProperties();
        props.Persistent = true;

        _channel.BasicPublish(exchange: string.Empty, routingKey: "todo-events", basicProperties: props, body: body);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
