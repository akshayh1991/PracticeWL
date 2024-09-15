using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    internal class Role
    {
        [Key]
        public ulong Id { get; set; }
        public string? Name { get; set; }
        public List<User> Users { get; set; } = [];

        public List<Zone> Zones { get; set; } = [];

        //****************Added By Akshay***********************

      //  public List<RoleUser> RoleUsers { get; set; } = new List<RoleUser>();
        public string? Description { get; set; }
        public bool IsLoggedOutType { get; set; }
    }
}
