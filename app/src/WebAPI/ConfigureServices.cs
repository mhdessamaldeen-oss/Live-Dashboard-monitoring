using Application.Interfaces;
using WebAPI.DependencyInjection;
using WebAPI.Middleware;
using WebAPI.Services;

namespace WebAPI;

public static class ConfigureServices
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options => {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
        services.AddHttpContextAccessor();
        services.AddTransient<GlobalExceptionHandlerMiddleware>();
        
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<INotificationService, NotificationService>();

        services.AddSwaggerDocumentation();
        services.AddAuthServices(configuration);
        services.AddAppHealthChecks(configuration);

        return services;
    }
}
