namespace SecMan.Interfaces.BL
{
    public interface IEncryptionDecryption
    {
        string EncryptPassword(string password, bool IsLegacy);
    }
}
