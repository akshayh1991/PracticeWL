using System.ComponentModel.DataAnnotations;

namespace SecMan.Model
{
    public class AddRoleDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsLoggedOutType { get; set; }
        public List<ulong> LinkUsers { get; set; } = [];
    }

    public class GetRoleDto
    {
        public ulong Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string IsLoggedOutType { get; set; } = string.Empty;
        public int NoOfUsers { get; set; }
    }
}
