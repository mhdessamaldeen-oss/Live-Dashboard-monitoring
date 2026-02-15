using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Infrastructure.Common;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Scans provided assemblies for classes implementing IScopedService, ITransientService, or ISingletonService
    /// and registers them automatically with their implemented interfaces.
    /// </summary>
    public static IServiceCollection AddDynamicServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
                .ToList();

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();
                
                // We filter out our own marker interfaces and focus on business/infrastructure interfaces
                var businessInterfaces = interfaces
                    .Where(i => i != typeof(IScopedService) && 
                               i != typeof(ITransientService) && 
                               i != typeof(ISingletonService))
                    .ToList();

                // 1. Scoped
                if (typeof(IScopedService).IsAssignableFrom(type))
                {
                    foreach (var @interface in businessInterfaces)
                    {
                        services.AddScoped(@interface, type);
                    }
                    services.AddScoped(type);
                }

                // 2. Transient
                if (typeof(ITransientService).IsAssignableFrom(type))
                {
                    foreach (var @interface in businessInterfaces)
                    {
                        services.AddTransient(@interface, type);
                    }
                    services.AddTransient(type);
                }

                // 3. Singleton
                if (typeof(ISingletonService).IsAssignableFrom(type))
                {
                    foreach (var @interface in businessInterfaces)
                    {
                        services.AddSingleton(@interface, type);
                    }
                    services.AddSingleton(type);
                }
            }
        }

        return services;
    }
}
