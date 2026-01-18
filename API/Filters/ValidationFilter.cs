using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PutZige.Application.DTOs.Common;
using PutZige.Application.Common.Messages;
using System.Linq;
using System.Threading.Tasks;

namespace PutZige.API.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                context.Result = new BadRequestObjectResult(
                    ApiResponse<object>.Error(ErrorMessages.Validation.ValidationFailed, errors, 400));
                return;
            }

            await next();
        }
    }
}
