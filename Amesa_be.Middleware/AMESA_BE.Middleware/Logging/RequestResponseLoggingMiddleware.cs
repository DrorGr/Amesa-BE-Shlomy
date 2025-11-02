using AMESA_be.common.Extensions;
using AMESA_be.common.Enums;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace AMESA_be.Middleware.Logging
{
    public class RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger = logger;
        
        public async Task Invoke(HttpContext context)
        {
            //Get username
            var userId = context.User.Identity?.IsAuthenticated is true
                ? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                : "anonymous";
            LogContext.PushProperty("UserID", userId);

            var userName = context.User.Identity?.IsAuthenticated is true ? context.User.Identity.Name : "anonymous";
            LogContext.PushProperty("UserName", userName);

            //Get remote IP address  
            var requestAddress = context.GetRequestAddress();
            LogContext.PushProperty("IP", requestAddress);

            //Get remote LoggedInSession Id
            //var sessionId = context.Request.Headers["SessionId"].ToString();
            var sessionId = context.User.GetClaimValue(AMESAClaimTypes.SessionId.ToString());
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                // on gateway there is no session id
                sessionId = requestAddress;
            }

            LogContext.PushProperty("LoggedInSessionId", sessionId);

            var method = context.Request.Method;
            var url = context.Request.GetDisplayUrl();

            _logger.LogInformation("Handling RequestData: [{Method}] {Url} ", method, url);

            await _next(context);

            _logger.LogInformation("Finished RequestData: [{Method}] {Url} {Response}", method, url,
                context.Response.StatusCode);
        }
    }
}
