// using FluentValidation;
// using Microsoft.AspNetCore.Diagnostics;
// using Microsoft.AspNetCore.Mvc;

// namespace TmsApi.Api.ExceptionHandlers;

// public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
// {
//     public async ValueTask<bool> TryHandleAsync(
//         HttpContext httpContext,
//         Exception exception,
//         CancellationToken ct
//     )
//     {
//         var (status, title, detail, errors) = exception switch
//         {
//             ValidationException ve => (
//                 StatusCodes.Status400BadRequest,
//                 "Validation failed",
//                 "One or more fields are invalid. See errors for details.",
//                 (IDictionary<string, string[]>?)
//                     ve
//                         .Errors.GroupBy(e => e.PropertyName)
//                         .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
//             ),
//             _ => (
//                 StatusCodes.Status500InternalServerError,
//                 "Server error",
//                 $"An unexpected error occurred. Trace ID: {httpContext.TraceIdentifier}",
//                 null
//             ),
//         };

//         if (status == StatusCodes.Status500InternalServerError)
//             logger.LogError(
//                 exception,
//                 "Unhandled exception (trace={TraceId})",
//                 httpContext.TraceIdentifier
//             );

//         var problem = new ProblemDetails
//         {
//             Status = status,
//             Title = title,
//             Detail = detail,
//             Instance = httpContext.Request.Path,
//         };
//         if (errors is not null)
//             problem.Extensions["errors"] = errors;

//         httpContext.Response.StatusCode = status;
//         httpContext.Response.ContentType = "application/problem+json";
//         await httpContext.Response.WriteAsJsonAsync(problem, ct);
//         return true;
//     }
// }

using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Common.Exceptions;

namespace TmsApi.Api.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct
    )
    {
        var (status, title, detail, type, errors) = exception switch
        {
            NotFoundException nf => (nf.StatusCode, nf.Title, nf.Message, nf.Type, null),

            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                "One or more fields are invalid. See errors for details.",
                "https://tms.local/errors/validation",
                (IDictionary<string, string[]>?)
                    ve
                        .Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Server error",
                $"An unexpected error occurred. Trace ID: {httpContext.TraceIdentifier}",
                "https://tms.local/errors/internal_server_error",
                null
            ),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception (trace={TraceId})",
                httpContext.TraceIdentifier
            );
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = type,
            Instance = httpContext.Request.Path,
        };

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problem, ct);

        return true;
    }
}
