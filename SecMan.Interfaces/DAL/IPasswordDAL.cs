using SecMan.Model;
using static SecMan.Model.User;

namespace SecMan.Interfaces.DAL
{
    public  interface IPasswordDAL
    {
        Task<ulong> CheckForExistingUser(string oldPassword);
        Task<string> UpdatePasswordAsync(ulong userId, string newPassword);
    }
}
