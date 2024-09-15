using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Interfaces.DAL
{
    public  interface IPasswordDAL
    {
        Task<string> UpdatePasswordAsync(string oldPassword,string newPassword);
    }
}
