using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using SecMan.Data.Exceptions;
using SecMan.Model;
using Serilog;
using System.Net;

namespace UserAccessManagement.Middleware
{
    /// <summary>
    /// This is a middleware which will append common model to 500 internal server exceptions
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;


        /// <summary>
        /// This is an Middleware constructor for DI
        /// </summary>
        /// <param name="next"></param>
        /// <param name="env"></param>
        public ExceptionHandlingMiddleware(RequestDelegate next,
                                           IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        /// <summary>
        /// This method will triggered by pipeline when request comes this method
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }


        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            Log.Error(exception, "Internal Server Error Log on API {Type}", context.Request.GetEncodedUrl());
            CommonResponse response;
            if (exception is ConflictException)
            {
                response = new CommonResponse
                {
                    Status = HttpStatusCode.Conflict,
                    Type = context.Request.GetEncodedUrl(),
                    Title = "Conflict",
                    Detail = "A role with the same name already exists."
                };
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                string jsonResponse1 = JsonConvert.SerializeObject(response);
                return context.Response.WriteAsync(jsonResponse1);
            }
            else if (exception is BadRequestForLinkUsersNotExits)
            {
                response = new CommonResponse
                {
                    Status = HttpStatusCode.BadRequest,
                    Type = context.Request.GetEncodedUrl(),
                    Title = "BadRequest",
                    Detail = "Provided LinkUser/Users does'nt exits,Please Provide the valid LinkUser/Users list.."
                };
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                string jsonResponse1 = JsonConvert.SerializeObject(response);
                return context.Response.WriteAsync(jsonResponse1);
            }
            else if (exception is CommonBadRequestForRole)
            {
                response = new CommonResponse
                {
                    Status = HttpStatusCode.BadRequest,
                    Type = context.Request.GetEncodedUrl(),
                    Title = "Invalid Request",
                    Detail = "Provided input request parameter is not valid."
                };
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                string jsonResponse1 = JsonConvert.SerializeObject(response);
                return context.Response.WriteAsync(jsonResponse1);
            }
            else if (exception is UpdatingExistingNameException)
            {
                response = new CommonResponse
                {
                    Status = HttpStatusCode.BadRequest,
                    Type = context.Request.GetEncodedUrl(),
                    Title = "Invalid Request",
                    Detail = "Role Name is already present with this name"
                };
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                string jsonResponse1 = JsonConvert.SerializeObject(response);
                return context.Response.WriteAsync(jsonResponse1);
            }
            else
            {
                response = new CommonResponse
                {
                    Status = HttpStatusCode.InternalServerError,
                    Type = context.Request.GetEncodedUrl(),
                    Title = "Internal Server Error",
                    Detail = $"{exception.Message}"
                };
            }
            if (_env.IsDevelopment())
            {
                response.Detail = $"{exception.Message}\n {exception.StackTrace}";
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            string jsonResponse = JsonConvert.SerializeObject(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}