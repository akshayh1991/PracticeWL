using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SecMan.Interfaces.BL;
using SecMan.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.BL
{
    public class CurrentUserServices : ICurrentUserServices
    {
        private readonly IHttpContextAccessor _httpContext;

        public CurrentUserServices(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }

        public bool IsLoggedIn => _httpContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null;

        public UserDetails? UserDetails
        {
            get
            {
                var httpContext = _httpContext?.HttpContext;
                if (httpContext == null || httpContext.User == null)
                {
                    return null;
                }

                var userAttributes = httpContext.User.FindFirst("user_attributes")?.Value;
                if (string.IsNullOrEmpty(userAttributes))
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<UserDetails?>(userAttributes);
            }
        }
        public List<AppPermissions>? AppPermissions
        {
            get
            {
                var httpContext = _httpContext?.HttpContext;
                if (httpContext == null || httpContext.User == null)
                {
                    return null;
                }

                var apps = httpContext.User.FindFirst("apps")?.Value;
                if (string.IsNullOrEmpty(apps))
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<List<AppPermissions>?>(apps);
            }
        }

        public ulong UserId
        {
            get
            {
                var httpContext = _httpContext?.HttpContext;
                if (httpContext == null || httpContext.User == null)
                {
                    return 0;
                }
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return 0;
                }
                return Convert.ToUInt64(userId);
            }
        }


        public string UserName
        {
            get
            {
                var httpContext = _httpContext?.HttpContext;
                if (httpContext == null || httpContext.User == null)
                {
                    return string.Empty;
                }
                var userName = httpContext.User.FindFirst("sub");
                if (userName == null)
                {
                    return string.Empty;
                }
                return Convert.ToString(userName) ?? string.Empty;
            }
        }
    }
}
