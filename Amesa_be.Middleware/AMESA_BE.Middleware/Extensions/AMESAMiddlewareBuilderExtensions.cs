using AMESA_BE.Middleware.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace AMESA_BE.Middleware.Extensions
{
    public static class AMESAMiddlewareBuilderExtensions
    {
        public static IApplicationBuilder UseAMESAMiddleware(
            this IApplicationBuilder builder)
        {
            builder.UseMiddleware<ErrorHandlerMiddleware>();
            return builder;
        }
    }
}
