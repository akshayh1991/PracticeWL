using SecMan.Model;

namespace SecMan.Data.Repository
{
    public interface IPasswordRepository
    {
        Task<ulong> CheckForHashedToken(string hashedToken);
        Task<UserCredentialsDto> CheckForHashedTokenWithUserDetails(string hashedToken);
        Task<GetForgetPasswordDto> ForgetPasswordCredentials(string userName);
        Task<string> GetPasswordExpiryWarningValue(string name);
        Task<List<string>> GetRecentPasswordsAsync(ulong userId);
        Task<UserCredentialsDto> GetUserCredentials(string userName);
        Task<GetUserNamePasswordDto> GetUserNamePasswordFromEmailId(string email);
        Task<string> UpdateHashedUserNamePassword(ulong userId, string hashedToken);
        Task<string> UpdatePasswordAsync(ulong userId, string newPassword);
    }
}