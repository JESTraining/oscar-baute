using Microsoft.Extensions.DependencyInjection;
using OPS.Application.Common.Interfaces;
using OPS.Application.Services;

namespace OPS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISecurityService, SecurityService>();
        return services;
    }
}
