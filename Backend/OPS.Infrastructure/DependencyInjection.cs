using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OPS.Application.Interfaces;
using OPS.Infrastructure.Data;
using OPS.Infrastructure.Data.Repositories;
using OPS.Infrastructure.Messaging;

namespace OPS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        
        services.AddHostedService<InventoryCheckConsumer>();
        
        return services;
    }
}