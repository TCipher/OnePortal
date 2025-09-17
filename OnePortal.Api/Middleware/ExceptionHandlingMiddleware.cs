using FluentValidation;
using OnePortal.Application.Common;
using Serilog.Context;
using System.Net;

namespace OnePortal.API.Middleware
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
        {
            try
            {
                using (LogContext.PushProperty("RequestId", ctx.TraceIdentifier))
                using (LogContext.PushProperty("UserId", ctx.User?.Identity?.IsAuthenticated == true ? ctx.User.Identity.Name : null))
                {
                    await next(ctx);
                }
            }
            catch (Exception ex)
            {
                var (status, code, message, details) = MapException(ex);
                _logger.LogError(ex, "Unhandled exception: {Code}", code);

                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = (int)status;

                var envelope = ApiResponse<object>.Fail(code, message, details);
                await ctx.Response.WriteAsJsonAsync(envelope);
            }
        }

        private static (HttpStatusCode status, string code, string message, object? details) MapException(Exception ex)
        {
            return ex switch
            {
                ValidationException ve => (HttpStatusCode.BadRequest, "validation_error", "Validation failed",
                    new { errors = ve.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }) }),

                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "unauthorized", "Unauthorized", null),

                KeyNotFoundException => (HttpStatusCode.NotFound, "not_found", "Resource not found", null),

                InvalidOperationException ioe when ioe.Message.Contains("conflict", StringComparison.OrdinalIgnoreCase)
                    => (HttpStatusCode.Conflict, "conflict", ioe.Message, null),

                _ => (HttpStatusCode.InternalServerError, "internal_error", "An unexpected error occurred", null)
            };
        }
    }
}
