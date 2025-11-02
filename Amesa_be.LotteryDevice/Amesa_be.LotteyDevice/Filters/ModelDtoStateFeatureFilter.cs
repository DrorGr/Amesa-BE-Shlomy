using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AMESA_BE.Models;

namespace AMESA_BE.Filters
{
    public class ModelDtoStateFeatureFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = new Dictionary<string, string[]>();
                foreach (var state in context.ModelState)
                {
                    if (state.Value.Errors.Any())
                    {
                        errors[state.Key] = state.Value.Errors.Select(e => e.ErrorMessage).ToArray();
                    }
                }

                var response = new GeneralActionResponse<object>
                {
                    Success = false,
                    Message = "One or more validation errors occurred.",
                    Data = errors
                };

                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Nothing to do after action execution
        }
    }
}