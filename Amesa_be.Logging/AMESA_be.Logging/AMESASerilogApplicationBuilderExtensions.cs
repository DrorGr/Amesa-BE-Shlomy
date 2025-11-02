using AMESA_be.Logging.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using AMESA_be.Middleware.Logging;
using Serilog.Exceptions;

namespace AMESA_be.Logging.Serilog
{
    public static class AMESASerilogApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds serilog as the logging framework for this Host. 
        /// A configuration file named 'serilog.json' MUST be on project top level folder
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHostBuilder UseAMESASerilog(
            this IHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("serilog.json")
            .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithExceptionDetails()
                .CreateLogger();

            builder.UseAMESASerilog();

            return builder;
        }

        public static IApplicationBuilder UseAMESASerilog(
           this IApplicationBuilder builder)
        {
            builder.UseMiddleware<RequestResponseLoggingMiddleware>();
            builder.UseSerilogRequestLogging(opts => opts.EnrichDiagnosticContext = LogHelper.EnrichFromRequest);


            return builder;
        }

        private static void ShowHeaders(IApplicationBuilder app)
        {
            app.Run(async context => {
                var requestHeaders = string.Join("<br/>", AMESASerilogScopedLoggingMiddleware.RequestHeaders.OrderBy(x => x));
                var responseHeaders = string.Join("<br />", AMESASerilogScopedLoggingMiddleware.ResponseHeaders.OrderBy(x => x));
                var output = $"<h2>Unique request headers</h2>{requestHeaders} <h2>Unique response headers</h2> {responseHeaders}";
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(output);
            });
        }
    }
}
