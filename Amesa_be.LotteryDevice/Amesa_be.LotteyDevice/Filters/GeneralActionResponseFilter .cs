using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AMESA_BE.Models;

namespace AMESA_BE.Filters
{
    public class GeneralActionResponseFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            if (resultContext.Result is ObjectResult objectResult)
            {
                if (objectResult.Value is not GeneralActionResponse<object>)
                {
                    resultContext.Result = new OkObjectResult(new GeneralActionResponse<object>
                    {
                        Success = true,
                        Data = objectResult.Value
                    });
                }
            }
        }
    }
}