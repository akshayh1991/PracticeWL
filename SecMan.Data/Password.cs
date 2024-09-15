using Microsoft.EntityFrameworkCore;
using SecMan.Interfaces.DAL;
using System.Security.Policy;
using static SecMan.Model.User;

namespace SecMan.Data
{
    public class Password : IPasswordDAL
    {

        public async Task<string> UpdatePasswordAsync(ulong userId, string newPassword)
        {
            using var db = new SQLCipher.Db();
            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }
            user.Password = newPassword;
            await db.SaveChangesAsync();
            return user.Password;
        }


        public async Task<ulong> CheckForExistingUser(string oldPassword)
        {
            using var db = new SQLCipher.Db();
            var trimmedOldPassword = oldPassword.Trim();

            var existingUser = await db.Users
                .FirstOrDefaultAsync(x => x.Password == trimmedOldPassword);
            if (existingUser != null)
            {
                return existingUser.Id;
            }
            return 0;
        }




    }
}
