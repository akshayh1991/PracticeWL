using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Interfaces.BL
{
    public  interface IPasswordBL
    {
        Task<string> UpdatePasswordAsync(string oldPassword, string newPassword);
    }
}
