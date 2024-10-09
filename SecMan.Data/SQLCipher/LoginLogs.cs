using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class LoginLogs
    {
        [Key]
        public ulong LoginLogId { get; set; }

        public User? User { get; set; }

        public DateTime ActionDate { get; set; }

        public bool IsSuccessfullyLoggedIn { get; set; }
    }
}
