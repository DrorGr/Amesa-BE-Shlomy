using AMESA_be.common.Enums;
using AMESA_be.common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace AMESA_be.AMESAJWTAuthentication.Handlers
{
    public class JwtAMESAAuthorizationResultHandler(IJwtTokenManager jwtTokenManager) : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
        private readonly IJwtTokenManager _jwtTokenManager = jwtTokenManager;

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            //ToDo: add validate to iss claim 
            var expTime = context.User.GetClaimValue(AMESAClaimTypes.exp.ToString());
            var sessionId = context.User.GetClaimValue(AMESAClaimTypes.SessionId.ToString());

            if (sessionId != null && expTime != null)
            {
                var expTimeAsDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expTime)).ToLocalTime();
                var now = DateTimeOffset.Now;
                if (now > expTimeAsDate.AddMinutes(-10) && now < expTimeAsDate)
                {
                    //refresh token
                    var token = _jwtTokenManager.GenerateAccessToken(context.User.Claims,
                        DateTimeOffset.Now.AddHours(1).DateTime);
                    if (token != null)
                    {
                        context.Response.Headers.Add(AMESAHeaders.Authorization.ToString(), $"Bearer {token}");
                    }
                }
            }
            else
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token expired or sessionId is not present in request.");
                return;
            }

            // Fall back to the default implementation.
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }

        public class Show404Requirement : IAuthorizationRequirement
        {
        }
    }
}
