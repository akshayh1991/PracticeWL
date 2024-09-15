using SecMan.Interfaces.BL;
using SecMan.Interfaces.DAL;
using SecMan.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.BL
{
    public class PasswordBL:IPasswordBL
    {
        private readonly IPasswordDAL _passwordDAL;
        private readonly IEncryptionDecryption _encryptionDecryption;

        public PasswordBL(IPasswordDAL passwordDAL,IEncryptionDecryption encryptionDecryption)
        {
            _passwordDAL = passwordDAL;
            _encryptionDecryption = encryptionDecryption;
        }

        public async Task<string> UpdatePasswordAsync(string oldPassword, string newPassword)
        {
            string encPassword=_encryptionDecryption.EncryptPassword(newPassword, true);
            var result = _passwordDAL.UpdatePasswordAsync(oldPassword, encPassword);
            return await result;
        }
    }
}
