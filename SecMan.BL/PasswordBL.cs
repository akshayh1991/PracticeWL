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
            string encOldPassword=_encryptionDecryption.EncryptPassword(oldPassword, true);
            ulong userId= await _passwordDAL.CheckForExistingUser1("$AESEncryption$686468676a7479686a67636a79746367636a7963686374797468796474636363$d4fab31b3ab4a49821a96201eb905f9a$ab31341dc94cc61b5ecc14ea4f1b4761");
            if (userId > 0)
            {
                string encNewPassword = _encryptionDecryption.EncryptPassword(newPassword, true);
                var trimmedNewPassword = encNewPassword.Trim();
                var result = _passwordDAL.UpdatePasswordAsync1(userId, trimmedNewPassword);
                //var result = _passwordDAL.UpdatePasswordAsync1(userId, "$AESEncryption$686468676a7479686a67636a79746367636a7963686374797468796474636363$c3cac7b13ea9bf963d5f6a4897b9cca5$17dde76ab23f8f74243ad5b899a6fd01");
            }
            return null;
        }
    }
}
