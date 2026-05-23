using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using retakebackend.consumer.Data;
using retakebackend.consumer.Messaging;
using retakebackend.consumer.Models;

namespace retakebackend.consumer.Services;

public class RabbitConsumerService(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<RabbitConsumerService> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMq:Host"] ?? "localhost",
            UserName = config["RabbitMq:Username"] ?? "guest",
            Password = config["RabbitMq:Password"] ?? "guest"
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        channel.QueueDeclare("todo-events", durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<TodoMessage>(Encoding.UTF8.GetString(ea.Body.ToArray()));
                if (message is not null) await ApplyMessage(message, stoppingToken);
                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error consuming rabbit message");
                channel.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
        };

        channel.BasicConsume("todo-events", autoAck: false, consumer);
        stoppingToken.Register(() => { channel.Dispose(); connection.Dispose(); });
        return Task.CompletedTask;
    }

    private async Task ApplyMessage(TodoMessage msg, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReplicaDbContext>();

        if (msg.EntityType == "TodoList")
        {
            if (msg.Action == "Deleted")
            {
                var existing = await db.TodoLists.FindAsync([msg.Id], ct);
                if (existing is not null) db.TodoLists.Remove(existing);
            }
            else
            {
                var payload = JsonSerializer.Deserialize<TodoList>(msg.PayloadJson);
                if (payload is null) return;
                var existing = await db.TodoLists.FirstOrDefaultAsync(x => x.Id == payload.Id, ct);
                if (existing is null) db.TodoLists.Add(payload); else existing.Title = payload.Title;
            }
        }
        if (msg.EntityType == "TodoItem")
        {
            if (msg.Action == "Deleted")
            {
                var existing = await db.TodoItems.FindAsync([msg.Id], ct);
                if (existing is not null) db.TodoItems.Remove(existing);
            }
            else
            {
                var payload = JsonSerializer.Deserialize<TodoItem>(msg.PayloadJson);
                if (payload is null) return;
                var existing = await db.TodoItems.FirstOrDefaultAsync(x => x.Id == payload.Id, ct);
                if (existing is null) db.TodoItems.Add(payload);
                else { existing.Title = payload.Title; existing.IsCompleted = payload.IsCompleted; existing.TodoListId = payload.TodoListId; }
            }
        }
        await db.SaveChangesAsync(ct);
    }
}
