using Application;
using Application.Interfaces;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using WebAPI;
using WebAPI.Hubs;
using WebAPI.Middleware;

System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebApiServices(builder.Configuration);

// SignalR with Redis backplane
var redisConn = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConn!, options => {
        options.Configuration.ChannelPrefix = "MonitoringApp";
    });

var app = builder.Build();

// Database migration and seeding (development only — production uses CI/CD migrations)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<Infrastructure.Persistence.ApplicationDbContext>();
        if (context.Database.IsSqlServer())
        {
            await context.Database.MigrateAsync();
        }
        await Infrastructure.Persistence.ApplicationDbContextSeed.SeedDefaultDataAsync(context);
        Infrastructure.Persistence.HangfireJobScheduler.ScheduleJobs(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database migration or seeding failed.");
    }
}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// TODO: Restrict origins per environment before deploying to production
app.UseCors(policy => policy
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    .SetIsOriginAllowed(host => true));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                exception = e.Value.Exception?.Message
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHub<MonitoringHub>("/hubs/monitoring");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.Run();

public partial class Program { }
