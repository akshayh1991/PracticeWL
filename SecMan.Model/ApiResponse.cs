using Newtonsoft.Json;
using System.Net;

namespace SecMan.Model
{
    public class ApiResponse
    {
        public ApiResponse()
        {

        }

        public ApiResponse(string message, HttpStatusCode statusCode)
        {
            Message = message;
            StatusCode = statusCode;
        }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("statusCode")]
        public HttpStatusCode StatusCode { get; set; }
    }



    public class ServiceResponse<T> : ApiResponse
    {
        public ServiceResponse()
        {

        }

        public ServiceResponse(string message, HttpStatusCode statusCode) : base(message, statusCode)
        {

        }

        public ServiceResponse(string message, HttpStatusCode statusCode, T? data) : base(message, statusCode)
        {
            Data = data;
        }

        [JsonProperty("date")]
        public T? Data { get; set; }
    }




    public class CommonResponse
    {
        public CommonResponse()
        {

        }

        public CommonResponse(string title, HttpStatusCode status, string detail)
        {
            Title = title;
            Status = status;
            Detail = detail;
        }

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("status")]
        public HttpStatusCode Status { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; } = string.Empty;
    }


    public class SuccessResponse<T> : CommonResponse
    {
        public SuccessResponse(string title, HttpStatusCode status = HttpStatusCode.OK, string detail = "")
            : base(title, status, detail)
        {
        }

        public T? Data { get; set; }
    }


    public class BadRequestResponse : CommonResponse
    {
        public BadRequestResponse(string title, string detail, List<InvalidParams>? invalidParams = null)
            : base(title, HttpStatusCode.BadRequest, detail)
        {
            InvalidParams = invalidParams;
        }

        [JsonProperty("invalidParams")]
        public List<InvalidParams>? InvalidParams { get; set; }
    }





    public class InvalidParams
    {
        public string In { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
