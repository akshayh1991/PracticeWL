using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SecMan.Data.Audit;
using SecMan.Model;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Windows.Forms.Design;

namespace UserAccessManagement.Filters
{
    /// <summary>
    /// This Is a ActionFilter, this ActionFilter will add api url for every API's Response
    /// </summary>
    public class AddRequestUrlToResponseFilter : IActionFilter
    {
        private string _requestUrl = string.Empty;
        private IAuditServices? _auditServices;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// This Method is trigged before execution of controller method
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            _stopwatch.Start();
            _requestUrl = context.HttpContext.Request.GetEncodedUrl();
            _auditServices = context.HttpContext.RequestServices.GetService<IAuditServices>();
        }


        /// <summary>
        /// This method is triggered after controller method is triggered
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            if (context.Result is ObjectResult objectResult && objectResult.Value is CommonResponse commonResponse)
            {
                commonResponse.Type = _requestUrl;

                context.Result = new ObjectResult(commonResponse)
                {
                    StatusCode = objectResult.StatusCode,
                    ContentTypes = objectResult.ContentTypes
                };
                AddAuditLog(context, commonResponse);
            }
            else if (context.Result is UnauthorizedResult unauthorizedObject)
            {
                Unauthorized jsonResponse = new Unauthorized
                {
                    Type = context.HttpContext?.Request?.GetEncodedUrl() ?? string.Empty,
                    Title = "Unauthorized",
                    Status = HttpStatusCode.Unauthorized,
                    Detail = "Invalid access token"
                };
                context.Result = new ObjectResult(jsonResponse)
                {
                    StatusCode = unauthorizedObject.StatusCode
                };
                AddAuditLog(context, jsonResponse);
            }
            else if (context.Result is ForbidResult)
            {
                Forbidden jsonResponse = new()
                {
                    Type = context.HttpContext?.Request?.GetEncodedUrl() ?? string.Empty,
                    Title = "Forbidden",
                    Status = HttpStatusCode.Forbidden,
                    Detail = "Permission Denied"
                };
                context.Result = new ObjectResult(jsonResponse)
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
                AddAuditLog(context, jsonResponse);
            }
            else
            {
                AddAuditLog(context);
            }
        }


        private void AddAuditLog(ActionExecutedContext context, CommonResponse? commonResponse = null)
        {
            if (_auditServices is not null)
            {
                _stopwatch.Stop();
                AuditDto auditObject = new AuditDto
                {
                    Description = context.HttpContext.Request.Path,
                    APIResponseCode = context.HttpContext?.Response.StatusCode.ToString(),
                    APIDescription = commonResponse?.Title ?? "Success",
                    Status = Convert.ToString(commonResponse?.Status ??
                                             ((HttpStatusCode?)(context.HttpContext?.Response.StatusCode))),
                    EntityName = context.RouteData.Values["controller"]?.ToString(),
                    Component = context.RouteData.Values["action"]?.ToString(),
                    ServerIP = context.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                    ResponseTime = _stopwatch.Elapsed.TotalSeconds,
                };
                if (context.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier) is not null)
                {
                    auditObject.ActionBy = Convert.ToString(context.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                }
                _auditServices.APIAudit(auditObject);
            }
        }
    }
}
