using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OPS.Application.Events;
using OPS.Application.Interfaces;
using RabbitMQ.Client;

namespace OPS.Infrastructure.Messaging;

/// <summary>
/// Publishes OrderCreatedEvent to the "order.created" queue. Opens a fresh connection/channel per
/// publish call.
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher
{
    public const string QueueName = "order.created";

    private readonly IConfiguration configuration;

    public RabbitMqEventPublisher(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void Publish(OrderCreatedEvent evt)
    {
        var factory = CreateConnectionFactory();

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var body = JsonSerializer.SerializeToUtf8Bytes(evt);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        channel.BasicPublish(exchange: string.Empty, routingKey: QueueName, basicProperties: properties, body: body);
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        var section = configuration.GetSection("RabbitMq");
        
        return new ConnectionFactory
        {
            HostName = section["Host"] ?? "localhost",
            UserName = section["Username"] ?? "guest",
            Password = section["Password"] ?? "guest"
        };
    }
}