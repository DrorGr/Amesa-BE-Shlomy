using Microsoft.AspNetCore.Http;
using Serilog;

namespace AMESA_be.Logging.Helpers
{
    public static class LogHelper
    {
        /// <summary>
        /// Enriches Serilog's DiagnosticContext with properties from the HTTP request.
        /// </summary>
        /// <param name="diagnosticContext">The diagnostic context to enrich.</param>
        /// <param name="httpContext">The HTTP context to get information from.</param>
        public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestPath", httpContext.Request.Path);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);

            if (httpContext.Request.QueryString.HasValue)
            {
                diagnosticContext.Set("RequestQueryString", httpContext.Request.QueryString.Value);
            }
        }
    }
}