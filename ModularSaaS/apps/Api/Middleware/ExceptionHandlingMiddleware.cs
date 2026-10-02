using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ModularSaaS.Api.Common;
using ModularSaaS.Application.Shared.Exceptions;

namespace ModularSaaS.Api.Middleware;

internal sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = ProblemDetailsConstants.ContentType;

        var problem = exception switch
        {
            TenantRequiredException ex => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = ProblemDetailsConstants.TenantRequiredTitle,
                Detail = ex.Message,
                Type = ProblemDetailsConstants.TenantRequiredType
            },
            ValidationException ex => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = ProblemDetailsConstants.ValidationTitle,
                Detail = ex.Message,
                Type = ProblemDetailsConstants.ValidationType,
                Extensions = { [ProblemDetailsConstants.ErrorsKey] = ex.Errors }
            },
            NotFoundException ex => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = ProblemDetailsConstants.NotFoundTitle,
                Detail = ex.Message,
                Type = ProblemDetailsConstants.NotFoundType
            },
            UnauthorizedException ex => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = ProblemDetailsConstants.UnauthorizedTitle,
                Detail = ex.Message,
                Type = ProblemDetailsConstants.UnauthorizedType
            },
            var ex when ex.GetType().Name == ProblemDetailsConstants.DomainExceptionName => new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = ProblemDetailsConstants.BusinessRuleViolationTitle,
                Detail = ex.Message,
                Type = ProblemDetailsConstants.BusinessRuleViolationType
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = ProblemDetailsConstants.InternalServerErrorTitle,
                Detail = ProblemDetailsConstants.InternalServerErrorDetail,
                Type = ProblemDetailsConstants.InternalServerErrorType
            }
        };

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
