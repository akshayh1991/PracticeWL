using SecMan.Model;
using static SecMan.Model.User;

namespace SecMan.Interfaces.DAL
{
    public  interface IPasswordDAL
    {
        Task<ulong> CheckForExistingUser1(string oldPassword);
        Task<string> UpdatePasswordAsync1(ulong userId, string newPassword);
    }
}
