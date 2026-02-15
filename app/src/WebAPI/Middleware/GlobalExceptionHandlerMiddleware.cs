using Application.Common;
using Domain.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace WebAPI.Middleware;

public class GlobalExceptionHandlerMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = HttpStatusCode.InternalServerError;
        var errorMessage = "An error occurred while processing your request.";
        object? errors = null;

        switch (exception)
        {
            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                errorMessage = "Validation failed.";
                errors = validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                break;
            case EntityNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                errorMessage = exception.Message;
                break;
            case BusinessRuleException:
                statusCode = HttpStatusCode.Conflict;
                errorMessage = exception.Message;
                break;
            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                errorMessage = exception.Message;
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        
        if (errors != null)
        {
            var errorResult = new 
            {
                Succeeded = false,
                Error = errorMessage,
                ValidationErrors = errors
            };
            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResult));
        }

        var response = Result.Failure(errorMessage);
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
