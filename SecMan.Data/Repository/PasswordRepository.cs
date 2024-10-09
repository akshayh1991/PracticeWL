using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecMan.Data.SQLCipher;
using SecMan.Interfaces.BL;
using SecMan.Interfaces.DAL;
using SecMan.Model;
using Serilog;

namespace SecMan.Data.Repository
{
    public class PasswordRepository : IPasswordRepository
    {
        private readonly Db _context;

        public PasswordRepository( Db context)
        {
            _context = context;
        }
        public async Task<UserCredentialsDto> GetUserCredentials(string userName)
        {
            Log.Information("GetUserCredentials called for username: {Username}", userName);

            if (string.IsNullOrWhiteSpace(userName))
            {
                Log.Warning("Username is null or empty.");
                throw new ArgumentException("Username is required.", nameof(userName));
            }

            var trimmedUserName = userName.Trim();

            var existingUser = await _context.Users
                .Where(x => x.UserName == trimmedUserName)
                .Select(x => new UserCredentialsDto
                {
                    userId = x.Id,
                    Password = x.Password
                })
                .FirstOrDefaultAsync();

            if (existingUser == null)
            {
                Log.Warning("No user found for username: {Username}", trimmedUserName);
            }
            else
            {
                Log.Information("User found for username: {Username}", trimmedUserName);
            }

            return existingUser;
        }
        public async Task<string> UpdatePasswordAsync(ulong userId, string newPassword)
        {
            Log.Information("UpdatePasswordAsync called for userId: {UserId}", userId);

            if (userId == 0)
            {
                Log.Warning("Invalid userId: {UserId}", userId);
                throw new ArgumentException("Valid userId is required.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                Log.Warning("New password is null or empty.");
                throw new ArgumentException("New password is required.", nameof(newPassword));
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                Log.Warning("User not found for userId: {UserId}", userId);
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            user.Password = newPassword;
            await _context.SaveChangesAsync();

            await InsertPasswordHistoryAsync(userId, newPassword, DateTime.UtcNow);

            Log.Information("Password updated successfully for userId: {UserId}", userId);
            return user.Password;
        }
        public async Task<List<string>> GetRecentPasswordsAsync(ulong userId)
        {
            return await _context.PasswordHistories
                .Where(ph => ph.UserId == userId)
                .OrderByDescending(ph => ph.CreatedDate)
                .Take(3)
                .Select(ph => ph.Password)
                .ToListAsync();
        }
        private async Task InsertPasswordHistoryAsync(ulong userId, string password, DateTime changeDate)
        {
            var passwordHistory = new PasswordHistory
            {
                UserId = userId,
                Password = password,
                CreatedDate = changeDate
            };

            _context.PasswordHistories.Add(passwordHistory);
            await _context.SaveChangesAsync();
        }
        public async Task<GetForgetPasswordDto> ForgetPasswordCredentials(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                Log.Warning("Username is null or empty.");
                throw new ArgumentException("Username is required.", nameof(userName));
            }

            var trimmedUserName = userName.Trim();

            Log.Information("Attempting to retrieve credentials for user: {Username}", trimmedUserName);

            var existingUser = await _context.Users
                .Where(x => x.UserName == trimmedUserName)
                .Select(x => new GetForgetPasswordDto
                {
                    userId = x.Id,
                    domain = x.Domain,
                    userName = x.UserName,
                    password = x.Password,
                    emailId = x.Email
                })
                .FirstOrDefaultAsync();

            if (existingUser == null)
            {
                Log.Warning("No user found with the username: {Username}", trimmedUserName);
            }
            else
            {
                Log.Information("User found: {Username}", existingUser.userName);
            }

            return existingUser;
        }
        public async Task<string> UpdateHashedUserNamePassword(ulong userId, string hashedToken)
        {

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                Log.Warning("User not found for ID: {UserId}", userId);
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            user.ResetPasswordToken = hashedToken;
            user.ResetPasswordTokenExpiry = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            Log.Information("Updated hashed token for user: {UserId}", userId);
            return user.ResetPasswordToken;
        }
        public async Task<GetUserNamePasswordDto> GetUserNamePasswordFromEmailId(string email)
        {
            Log.Information("GetUserNamePasswordFromEmailId called for email: {Email}", email);

            if (string.IsNullOrWhiteSpace(email))
            {
                Log.Warning("Email is null or empty.");
                throw new ArgumentException("Email is required.", nameof(email));
            }

            var trimmedEmailId = email.Trim();

            var userCreds = await _context.Users
                .Where(x => x.Email == trimmedEmailId)
                .Select(x => new GetUserNamePasswordDto
                {
                    userName = x.UserName,
                    password = x.Password,
                    hashedUserNamePassword = x.ResetPasswordToken,
                    hashedUserNamePasswordTime = x.ResetPasswordTokenExpiry
                })
                .FirstOrDefaultAsync();

            if (userCreds == null)
            {
                Log.Warning("No user found for email: {Email}", trimmedEmailId);
            }
            else
            {
                Log.Information("User found for email: {Email}", trimmedEmailId);
            }

            return userCreds;
        }
        public async Task<string> GetPasswordExpiryWarningValue(string name)
        {
            string trimmedName = name.Trim();
            Log.Information("GetPasswordExpiryWarningValue called for name: {Name}", name);

            if (string.IsNullOrWhiteSpace(name))
            {
                Log.Warning("Name is null or empty.");
                throw new ArgumentException("Name is required.");
            }

            var result = await _context.SysFeatProps.FirstOrDefaultAsync(x => x.Name == trimmedName);

            if (result == null)
            {
                Log.Warning("No value found for name: {Name}", name);
                return string.Empty;
            }

            Log.Information("Value found for name: {Name}, Value: {Value}", name, result.Val);
            return result.Val;
        }
        public async Task<ulong> CheckForHashedToken(string hashedToken)
        {
            Log.Information("CheckForHashedToken called with hashedToken: {HashedToken}", hashedToken);

            if (string.IsNullOrWhiteSpace(hashedToken))
            {
                Log.Warning("Hashed token is null or empty.");
                throw new ArgumentException("Hashed token is required.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.ResetPasswordToken == hashedToken);

            if (user == null)
            {
                Log.Warning("No user found with hashed token: {HashedToken}", hashedToken);
                return 0;
            }

            Log.Information("User found for hashed token: {HashedToken}, UserId: {UserId}", hashedToken, user.Id);
            return user.Id;
        }
        public async Task<UserCredentialsDto> CheckForHashedTokenWithUserDetails(string hashedToken)
        {
            Log.Information("CheckForHashedTokenWithUserDetails called with hashedToken: {HashedToken}", hashedToken);

            if (string.IsNullOrWhiteSpace(hashedToken))
            {
                Log.Warning("Hashed token is null or empty.");
                throw new ArgumentException("Hashed token is required.", nameof(hashedToken));
            }

            Log.Information("Querying the database for user with hashed token.");

            var user = await _context.Users.FirstOrDefaultAsync(x => x.ResetPasswordToken == hashedToken);

            if (user != null)
            {
                Log.Information("User found with ID: {UserId}", user.Id);
                return new UserCredentialsDto
                {
                    userId = user.Id,
                    Password = user.Password
                };
            }

            Log.Information("No user found for the given hashed token.");
            return null;
        }

    }
}
