using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPS.Application.Events;
using OPS.Domain.Entities;
using OPS.Infrastructure.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OPS.Infrastructure.Messaging;
/// <summary>
/// Background service that listens to the "order.created" queue and performs an inventory check for each order. Updates the order status based on the inventory check result.
/// </summary>
public class InventoryCheckConsumer : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IConfiguration configuration;
    private readonly ILogger<InventoryCheckConsumer> logger;

    public InventoryCheckConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<InventoryCheckConsumer> logger)
    {
        this.scopeFactory = scopeFactory;
        this.configuration = configuration;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var section = configuration.GetSection("RabbitMq");
        var factory = new ConnectionFactory
        {
            HostName = section["Host"] ?? "localhost",
            UserName = section["Username"] ?? "guest",
            Password = section["Password"] ?? "guest",
            DispatchConsumersAsync = true
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: RabbitMqEventPublisher.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                await Task.Delay(5000);
                await HandleMessageAsync(ea);
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process an order.created message.");
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        channel.BasicConsume(queue: RabbitMqEventPublisher.QueueName, autoAck: false, consumer: consumer);

        logger.LogInformation("InventoryCheckConsumer listening on '{Queue}'.", RabbitMqEventPublisher.QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        var evt = JsonSerializer.Deserialize<OrderCreatedEvent>(ea.Body.Span);
        if (evt is null)
        {
            logger.LogWarning("Received an order.created message that could not be deserialized.");
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == evt.OrderId);
        if (order is null)
        {
            logger.LogWarning("order.created referenced Order {OrderId}, which no longer exists.", evt.OrderId);
            return;
        }

        // Simulate an inventory check: random number
        var isAvailable = Random.Shared.Next(0, 2) == 1;

        dbContext.InventoryChecks.Add(new InventoryCheck
        {
            OrderId = order.Id,
            CheckDate = DateTimeOffset.UtcNow,
            IsAvailable = isAvailable
        });

        order.Status = isAvailable ? OrderStatus.Processing : OrderStatus.Failed;

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Inventory check for {OrderNumber}: {Result} -> status is now {Status}.",
            evt.OrderNumber,
            isAvailable ? "available" : "unavailable",
            order.Status);
    }
}