using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    internal class Zone
    {
        [Key]
        public ulong Id { get; set; }
        public string? Name { get; set; }

        public List<Role> Roles { get; set; } = [];

    }
}
