using Newtonsoft.Json;

namespace SecMan.Model
{
    public class BaseEntity
    {
        public int Id { get; set; }
    }

    public class CreateUser : UpdateUser
    {
        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;

        [JsonProperty("firstLogin")]
        public bool FirstLogin { get; set; } = true;
    }


    public class UpdateUser
    {
        [JsonProperty("username")]
        public string? Username { get; set; }

        [JsonProperty("domain")]
        public string? Domain { get; set; }

        [JsonProperty("firstName")]
        public string? FirstName { get; set; }

        [JsonProperty("lastName")]
        public string? LastName { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("language")]
        public string? Language { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("isLegacy")]
        public bool IsLegacy { get; set; } = false;

        [JsonProperty("roles")]
        public List<ulong> Roles { get; set; } = [];

        [JsonProperty("userAttributes")]
        public List<UserAttributeDto> UserAttributes { get; set; } = [];

        [JsonProperty("isPasswordExpiryEnabled")]
        public bool IsPasswordExpiryEnabled { get; set; }

        [JsonProperty("passwordExpiryDate")]
        public DateTime PasswordExpiryDate { get; set; }

            [JsonProperty("resetPassword")]
            public bool ResetPassword { get; set; }

            [JsonProperty("hashedUserNamePassword")]
            public string? HashedUserNamePassword { get; set; }
            [JsonProperty("hashedUserNamePasswordTime")]
            public DateTime? HashedUserNamePasswordTime { get; set; }
        }

    public class RoleModel
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; } = string.Empty;

        [JsonProperty("isLoggedOutType")]
        public bool IsLoggedOutType { get; set; }

        [JsonProperty("linkUsers")]
        public List<ulong> LinkUsers { get; set; } = [];
    }

    public class UserAttributeDto
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;
    }


    public class User : UpdateUser
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("inactiveDate")]
        public DateTime? InactiveDate { get; set; }

        [JsonProperty("isRetired")]
        public bool IsRetired { get; set; }

        [JsonProperty("retiredDate")]
        public DateTime RetiredDate { get; set; }

        [JsonProperty("isLocked")]
        public bool IsLocked { get; set; }

        [JsonProperty("lockedDate")]
        public DateTime LockedDate { get; set; }

        [JsonProperty("lockedReason")]
        public string LockedReason { get; set; } = string.Empty;

        [JsonProperty("lastLogin")]
        public DateTime LastLogin { get; set; }

        [JsonProperty("roles")]
        public new List<RoleModel> Roles { get; set; } = [];
    }


    public class UsersFilterDto
    {
        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("role")]
        public List<string> Role { 
            get => _roles.Select(x => x.ToLower().Replace(" ",string.Empty)).ToList();
            set => _roles = value; 
        }
        private List<string> _roles = [];

        [JsonProperty("status")]
        public List<string> Status
        {
            get => _status.Select(x => x.ToLower().Replace(" ", string.Empty)).ToList();
            set => _status = value;
        }
        private List<string> _status = [];

        [JsonProperty("offset")]
        public int Offset { get; set; } = 0;

        [JsonProperty("limit")]
        public int Limit { get; set; } = 500;
    }



    public class UsersWithCountDto
    {
        [JsonProperty("users")]
        public List<User> Users { get; set; } = [];

        [JsonProperty("userCount")]
        public int UserCount { get; set; }
    }
}
