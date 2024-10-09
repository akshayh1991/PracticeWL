using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Model
{

    public class LoginRequest
    {
        [JsonProperty("username")]
        public required string Username { get; set; }

        [JsonProperty("password")]
        public required string Password { get; set; }
    }




    public class LoginResponse
    {
        [JsonProperty("token")]
        public string? Token { get; set; }

        [JsonProperty("expiresIn")]
        public DateTimeOffset ExpiresIn { get; set; }
    }


    public class LoginServiceResponse
    {
        [JsonProperty("token")]
        public string? Token { get; set; }

        [JsonProperty("ssoSessionId")]
        public string? SSOSessionId { get; set; }

        [JsonProperty("expiresIn")]
        public double ExpiresIn { get; set; }
    }



    public class JwtTokenOptions
    {
        public const string JWTTokenValue = "JWT";
        public string? ValidIssuer { get; set; }
        public string? ValidAudience { get; set; }
        public string SecretKey { get; set; } = string.Empty;
        public double TokenExpireTime { get; set; }
    }


    public class UserDetails
    {
        public ulong Id { get; set; }
        public string? Username { get; set; }
        public string? Domain { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public string? Language { get; set; }
        public bool IsActive { get; set; }
        public bool IsRetired { get; set; }
        public bool IsExpired { get; set; }
        public bool IsLocked { get; set; }
        public bool IsLegacy { get; set; }
        public List<string?>? Roles { get; set; }
        public List<CustomAttributes> Custom_attributes { get; set; } = [];
    }

    public class CustomAttributes
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }

    public class AppPermissions
    {
        public string? Name { get; set; }
        public List<string>? Permission { get; set; }
    }





}
