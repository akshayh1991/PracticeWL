using SecMan.Interfaces.DAL;

namespace SecMan.Data
{
    public class Password : IPasswordDAL
    {
        public async Task<string> UpdatePasswordAsync(string oldPassword, string newPassword)
        {
            using var db = new SQLCipher.Db();
            var user = new SecMan.Data.SQLCipher.User
            {
                Id=6,
                Password = newPassword
            };
            db.Update(user);
            db.SaveChanges();
            return user.Password;
        }
    }
}
