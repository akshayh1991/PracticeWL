using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Extensions;
using SecMan.Data.Audit;
using SecMan.Model;
using System.Net;

namespace UserAccessManagement.Middleware
{
    /// <summary>
    /// 
    /// </summary>
    public class JwtBearerEventsMiddleware
    {
        private readonly IAuditServices _auditServices;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="auditServices"></param>
        public JwtBearerEventsMiddleware(IAuditServices auditServices)
        {
            _auditServices = auditServices;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task OnChallenge(JwtBearerChallengeContext context)
        {
            context.Response.StatusCode = 401;

            Unauthorized jsonResponse = new Unauthorized
            {
                Type = context.HttpContext?.Request?.GetEncodedUrl() ?? string.Empty,
                Title = "Unauthorized",
                Status = HttpStatusCode.Unauthorized,
                Detail = "Invalid access token"
            };
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(jsonResponse);

            context.HandleResponse();
            AddAuditLog(context, jsonResponse);
        }

        private void AddAuditLog(JwtBearerChallengeContext context, CommonResponse? commonResponse = null)
        {
            RouteData routeData = context.HttpContext.GetRouteData();
            if (_auditServices != null && context.HttpContext != null)
            {
                AuditDto auditObject = new AuditDto
                {
                    Description = context.HttpContext.Request.Path,
                    APIResponseCode = context.Response.StatusCode.ToString(),
                    APIDescription = commonResponse?.Title ?? "Unauthorized",
                    Status = Convert.ToString(commonResponse?.Status ?? ((HttpStatusCode?)context.HttpContext?.Response.StatusCode)),
                    EntityName = routeData.Values["controller"]?.ToString(),
                    Component = routeData.Values["action"]?.ToString(),
                    ServerIP = context.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                };
                _auditServices.APIAudit(auditObject);
            }
        }
    }
}
