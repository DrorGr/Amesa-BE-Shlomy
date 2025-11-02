using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace AMESA_be.Middleware.Attributes
{
    public class GatewayType : TypeFilterAttribute
    {
        public GatewayType(GatewayTypes[] itemType) : base(typeof(GatewayTypesAttribute))
        {
            Arguments = new object[] { itemType };
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        private class GatewayTypesAttribute : Attribute, IAsyncActionFilter
        {
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly ILogger<GatewayTypesAttribute> _logger;
            public GatewayTypesAttribute(ILogger<GatewayTypesAttribute> logger,
                IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
                _logger = logger;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var executedContext = await next();
            }
        }
    }

    public enum GatewayTypes
    {
        [Description("amesa-admin")]
        AdminStudio = 1,
        [Description("main-app")]
        Intsight = 2,
    }
}
