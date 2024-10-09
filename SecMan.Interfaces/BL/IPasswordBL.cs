using SecMan.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Interfaces.BL
{
    public interface IPasswordBl
    {
        Task<string> UpdatePasswordAsync(string userName,string oldPassword, string newPassword);
        Task<string> GenerateHashedToken(string userNamePassword);
        Task<GetForgetPasswordDto> ForgetPasswordAsync(string userName);
        Task<bool> GetUserNamePasswordAsync(string email,string token);
        Task<bool> CheckForHashedToken(string token, string newPassword);
    }
}
