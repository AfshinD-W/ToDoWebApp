using System.Net;
using System.Text.Json;
using SSToDo.Utilities;

namespace SSToDo.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var serviceResponse = new ServiceResponse<string>(exception.InnerException?.Message ?? exception.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var json = JsonSerializer.Serialize(serviceResponse);

            return context.Response.WriteAsync(json);
        }
    }
}
