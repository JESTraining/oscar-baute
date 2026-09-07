using Microsoft.Extensions.DependencyInjection;
using OPS.Application.Interfaces;
using OPS.Application.Services;

namespace OPS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISecurityService, SecurityService>();

        services.AddScoped<IOrderService, OrderService>();
        
        return services;
    }
}
