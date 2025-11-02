using AMESA_be.common.Contracts.GeneralResponse;
using AMESA_BE.Middleware.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace AMESA_BE.Middleware.Filters;

public class GeneralActionResponseFilter(IConfiguration configuration, IHttpContextAccessor contextAccessor) : IActionFilter
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception != null || context.Result == null)
        {
            return;
        }

        ApiResponse<object> apiResponse = null!;

        if (context.Result is NoContentResult noContentResult)
        {
            apiResponse = new ApiResponse<object>
            {
                Message = ResponseMessageEnum.NotContent.GetDescription(),
                Data = null!,
                Code = (int)(noContentResult?.StatusCode ?? 0),
                Version = GetApiVersion()
            };

            context.Result = new ObjectResult(apiResponse);
        }
        else if (context.Result is ObjectResult badRequestObjectResult && badRequestObjectResult.StatusCode >= 400)
        {

            apiResponse = new ApiResponse<object>
            {
                Message = String.IsNullOrEmpty(badRequestObjectResult.Value?.ToString()) ? ResponseMessageEnum.NotContent.GetDescription() : badRequestObjectResult.Value.ToString(),
                Data = null!,
                Code = badRequestObjectResult?.StatusCode ?? 400,
                Version = GetApiVersion(),
                IsError = true
            };

            var code = badRequestObjectResult?.StatusCode ?? 400;
            if (code >= 500)
                code = 400;
            context.Result = new ObjectResult(apiResponse)
            {
                StatusCode = code
            };
        }
        else if (context.Result is ObjectResult objectResult)
        {

            apiResponse = new ApiResponse<object>
            {
                Message = ResponseMessageEnum.Success.GetDescription(),
                Data = objectResult.Value!,
                Code = objectResult?.StatusCode ?? 0,
                Version = GetApiVersion()
            };

            if (objectResult?.Value == null)
            {
                apiResponse.Message = ResponseMessageEnum.NotContent.GetDescription();
                apiResponse.Code = StatusCodes.Status204NoContent;
            }

            context.Result = new ObjectResult(apiResponse);
        }
    }

    private string GetApiVersion()
    {
        //Extract Environment Variable from docker compose file (or docker file)
        //BUILD_VERSION example: "4.0.0-92"
        var buildVersion = Environment.GetEnvironmentVariable("BUILD_VERSION") ?? string.Empty;
        if (string.IsNullOrEmpty(buildVersion))
        {
            buildVersion = Environment.GetEnvironmentVariable("BUILD_VERSION_ENV") ?? string.Empty;
        }
        //Manipulate BUILD_VERSION string to be without the project's last version compiled  
        string delimiter = "-";
        string buildVersionTruncated = string.Empty;
        int lastIndexAddress = buildVersion.LastIndexOf(delimiter);
        if (lastIndexAddress != -1 && lastIndexAddress + delimiter.Length < buildVersion.Length)
        {
            buildVersionTruncated = buildVersion.Substring(0, lastIndexAddress);
        }
        //return value for example: BUILD_VERSION="4.0.0"
        return String.IsNullOrEmpty(buildVersionTruncated) ? buildVersion : buildVersionTruncated;
        //string strVer = _configuration.GetSection("Version:Key").Get<string>();
        //return string.IsNullOrEmpty(strVer) ? "1.0.0.0" : strVer;
    }
}
