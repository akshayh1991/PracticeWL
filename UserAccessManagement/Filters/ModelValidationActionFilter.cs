using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SecMan.Model;

namespace UserAccessManagement.Filters
{
    /// <summary>
    /// 
    /// </summary>
    public class ModelValidationActionFilter : IActionFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                List<InvalidParams?> errors = context.ModelState
                        .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                        .Select(x =>
                        {
                            string paramSource = GetParameterSource(context, x.Key);

                            InvalidParams? res = null;
                            if (x.Value is not null)
                            {
                                res = new InvalidParams
                                {
                                    In = paramSource,
                                    Name = x.Key,
                                    Reason = string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))
                                };
                            }
                            return res;
                        })
                        .Where(x => x!= null)
                        .ToList();

                BadRequest result = new BadRequest("Invalid Request", "Provided input request parameter is not valid.", errors);
                result.Type = context.HttpContext.Request.GetEncodedUrl();

                context.Result = new BadRequestObjectResult(result);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetParameterSource(ActionExecutingContext context, string key)
        {
            Microsoft.AspNetCore.Mvc.Abstractions.ParameterDescriptor? actionDescriptor = context.ActionDescriptor.Parameters
                .FirstOrDefault(p => p.Name == key);

            if (actionDescriptor != null && actionDescriptor.BindingInfo != null)
            {
                if (actionDescriptor.BindingInfo.BindingSource == BindingSource.Body)
                    return "body";
                if (actionDescriptor.BindingInfo.BindingSource == BindingSource.Query)
                    return "query";
                if (actionDescriptor.BindingInfo.BindingSource == BindingSource.Header)
                    return "header";
            }

            // Fallback or unknown source
            return "unknown";
        }
    }
}
