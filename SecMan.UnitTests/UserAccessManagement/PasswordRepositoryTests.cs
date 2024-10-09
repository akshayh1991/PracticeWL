using Moq;
using SecMan.Data.SQLCipher;
using SecMan.Data.Repository;
using SecMan.Interfaces.DAL;
using SecMan.Model;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Moq.EntityFrameworkCore;

namespace SecMan.UnitTests.UserAccessManagement
{
    public class PasswordRepositoryTests : IDisposable
    {
        private readonly PasswordRepository _passwordRepository;
        private readonly Db _dbContext;

        public PasswordRepositoryTests()
        {
            // Setup in-memory database for testing
            var options = new DbContextOptionsBuilder<Db>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _dbContext = new Db(options, string.Empty);
            _dbContext.Database.EnsureCreated();

            // Initialize the PasswordRepository with the in-memory context
            _passwordRepository = new PasswordRepository(_dbContext);
        }

        public void Dispose()
        {
            // Clean up the database after each test
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        //-------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task GetUserCredentials_ShouldReturnUserCredentials_WhenUserExists()
        {
            // Arrange
            var userName = "testuser";
            var user = new SecMan.Data.SQLCipher.User
            {
                Id = 1,
                UserName = userName,
                Password = "hashedPassword"
            };

            // Clear the database
            Log.Information("Clearing existing users from the database.");
            _dbContext.Users.RemoveRange(_dbContext.Users);
            await _dbContext.SaveChangesAsync();

            // Add the user to the in-memory database
            Log.Information("Adding a new user with username: {UserName}", userName);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            Log.Information("Acting to retrieve credentials for user: {UserName}", userName);
            var result = await _passwordRepository.GetUserCredentials(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.userId);
            Assert.Equal("hashedPassword", result.Password);

            Log.Information("Retrieved user credentials successfully for username: {UserName}", userName);
        }

        [Fact]
        public async Task GetUserCredentials_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userName = "nonexistentuser";
            Log.Information("Testing GetUserCredentials with a nonexistent username: {UserName}", userName);

            // Act
            var result = await _passwordRepository.GetUserCredentials(userName);

            // Assert
            Assert.Null(result);
            Log.Information("GetUserCredentials returned null as expected for username: {UserName}", userName);
        }

        [Fact]
        public async Task GetUserCredentials_ShouldThrowArgumentException_WhenUsernameIsNull()
        {
            // Arrange
            string userName = null;
            Log.Information("Testing GetUserCredentials with a null username.");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _passwordRepository.GetUserCredentials(userName));
            Assert.Equal("Username is required. (Parameter 'userName')", exception.Message);
            Log.Warning("Expected exception thrown for null username: {Message}", exception.Message);
        }

        [Fact]
        public async Task GetUserCredentials_ShouldThrowArgumentException_WhenUsernameIsEmpty()
        {
            // Arrange
            string userName = "";
            Log.Information("Testing GetUserCredentials with an empty username.");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _passwordRepository.GetUserCredentials(userName));
            Assert.Equal("Username is required. (Parameter 'userName')", exception.Message);
            Log.Warning("Expected exception thrown for empty username: {Message}", exception.Message);
        }


        //--------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task UpdatePasswordAsync_ShouldThrowArgumentException_WhenUserIdIsZero()
        {
            // Arrange
            ulong userId = 0;
            string newPassword = "NewPassword123!";
            Log.Information("Testing UpdatePasswordAsync with userId: {UserId} and newPassword: {NewPassword}", userId, newPassword);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _passwordRepository.UpdatePasswordAsync(userId, newPassword));
            Assert.Equal("Valid userId is required. (Parameter 'userId')", exception.Message);
            Log.Warning("Expected exception thrown for userId: {UserId}. Exception message: {Message}", userId, exception.Message);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldThrowArgumentException_WhenNewPasswordIsEmpty()
        {
            // Arrange
            ulong userId = 1; // Assume a valid userId
            string newPassword = ""; // Invalid password
            Log.Information("Testing UpdatePasswordAsync with userId: {UserId} and empty newPassword.", userId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _passwordRepository.UpdatePasswordAsync(userId, newPassword));
            Assert.Equal("New password is required. (Parameter 'newPassword')", exception.Message);
            Log.Warning("Expected exception thrown for empty newPassword. Exception message: {Message}", exception.Message);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            ulong userId = 99; // Assume this userId does not exist
            string newPassword = "NewPassword123!";
            Log.Information("Testing UpdatePasswordAsync with non-existent userId: {UserId}", userId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _passwordRepository.UpdatePasswordAsync(userId, newPassword));
            Assert.Equal("User with ID 99 not found.", exception.Message);
            Log.Warning("Expected exception thrown for non-existent userId: {UserId}. Exception message: {Message}", userId, exception.Message);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldUpdatePassword_WhenUserExists()
        {
            // Arrange
            ulong userId = 1;
            var user = new SecMan.Data.SQLCipher.User
            {
                Id = userId,
                UserName = "testuser",
                Password = "oldPassword"
            };

            // Log the setup
            Log.Information("Setting up test for UpdatePasswordAsync with userId: {UserId}, existing password: {OldPassword}", userId, user.Password);

            // Add the user to the in-memory database
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            string newPassword = "NewPassword123!";
            Log.Information("Testing UpdatePasswordAsync with new password: {NewPassword}", newPassword);

            // Act
            string updatedPassword = await _passwordRepository.UpdatePasswordAsync(userId, newPassword);

            // Assert
            var updatedUser = await _dbContext.Users.FindAsync(userId); // Retrieve the user again to check the updated password
            Assert.Equal(newPassword, updatedPassword);
            Assert.Equal(newPassword, updatedUser.Password); // Verify that the user's password has been updated

            Log.Information("Password updated successfully for userId: {UserId}. New Password: {NewPassword}", userId, updatedPassword);
        }




        //--------------------------------------------------------------------------------------------------

        [Fact]
        public async Task GetRecentPasswordsAsync_ShouldReturnLastThreePasswords_WhenUserHasRecentPasswords()
        {
            // Arrange
            ulong userId = 1;
            var passwordHistories = new List<PasswordHistory>
                {
                    new PasswordHistory { UserId = userId, Password = "Password1", CreatedDate = DateTime.UtcNow.AddDays(-1) },
                    new PasswordHistory { UserId = userId, Password = "Password2", CreatedDate = DateTime.UtcNow.AddDays(-2) },
                    new PasswordHistory { UserId = userId, Password = "Password3", CreatedDate = DateTime.UtcNow.AddDays(-3) },
                    new PasswordHistory { UserId = userId, Password = "Password4", CreatedDate = DateTime.UtcNow.AddDays(-4) }
                };

            // Add the password histories to the in-memory database
            foreach (var history in passwordHistories)
            {
                _dbContext.PasswordHistories.Add(history);
            }
            await _dbContext.SaveChangesAsync();

            Log.Information("Testing GetRecentPasswordsAsync for userId: {UserId} with recent passwords.", userId);

            // Act
            var recentPasswords = await _passwordRepository.GetRecentPasswordsAsync(userId);

            // Assert
            Assert.Equal(3, recentPasswords.Count);
            Assert.Contains("Password1", recentPasswords);
            Assert.Contains("Password2", recentPasswords);
            Assert.Contains("Password3", recentPasswords);

            Log.Information("Expected result: Last three passwords returned for userId: {UserId}. Passwords: {RecentPasswords}", userId, recentPasswords);
        }



        [Fact]
        public async Task GetRecentPasswordsAsync_ShouldReturnEmptyList_WhenUserHasNoPasswordHistory()
        {
            // Arrange
            ulong userId = 2; // Assume this user has no passwords
            Log.Information("Testing GetRecentPasswordsAsync for userId: {UserId}", userId);

            // Act
            var recentPasswords = await _passwordRepository.GetRecentPasswordsAsync(userId);

            // Assert
            Assert.Empty(recentPasswords);
            Log.Information("Expected result: Empty list returned for userId: {UserId}", userId);
        }



        //----------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task ForgetPasswordCredentials_ShouldReturnUserCredentials_WhenUserExists()
        {
            // Arrange
            var userName = "testuser";
            var user = new SecMan.Data.SQLCipher.User
            {
                Id = 1,
                UserName = userName,
                Password = "hashedPassword",
                Email = "testuser@example.com",
                Domain = "example.com"
            };

            // Add the user to the in-memory database
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            Log.Information("Testing ForgetPasswordCredentials for user: {UserName}", userName);

            // Act
            var result = await _passwordRepository.ForgetPasswordCredentials(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.userId);
            Assert.Equal(user.Password, result.password);
            Assert.Equal(user.Email, result.emailId);
            Assert.Equal(user.Domain, result.domain);
            Assert.Equal(userName, result.userName);

            Log.Information("User credentials retrieved for user {UserName}: {@Result}", userName, result);
        }

        [Fact]
        public async Task ForgetPasswordCredentials_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userName = "nonexistentuser";
            Log.Information("Testing ForgetPasswordCredentials for non-existent user: {UserName}", userName);

            // Act
            var result = await _passwordRepository.ForgetPasswordCredentials(userName);

            // Assert
            Assert.Null(result);
            Log.Information("Expected result: Null returned for user: {UserName}", userName);
        }

        [Fact]
        public async Task ForgetPasswordCredentials_ShouldThrowArgumentException_WhenUsernameIsNull()
        {
            // Arrange
            string userName = null;
            Log.Information("Testing ForgetPasswordCredentials with null username.");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordRepository.ForgetPasswordCredentials(userName));

            Assert.Equal("Username is required. (Parameter 'userName')", exception.Message);
            Log.Warning("Expected exception thrown: {Message}", exception.Message);
        }

        [Fact]
        public async Task ForgetPasswordCredentials_ShouldThrowArgumentException_WhenUsernameIsEmpty()
        {
            // Arrange
            string userName = "";
            Log.Information("Testing ForgetPasswordCredentials with empty username.");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordRepository.ForgetPasswordCredentials(userName));

            Assert.Equal("Username is required. (Parameter 'userName')", exception.Message);
            Log.Warning("Expected exception thrown: {Message}", exception.Message);
        }


        //----------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task UpdateHashedUserNamePassword_ShouldUpdateHashedToken_WhenUserExists()
        {
            // Arrange
            var userId = 1ul; // Use ulong
            var user = new SecMan.Data.SQLCipher.User
            {
                Id = userId,
                UserName = "testuser",
                ResetPasswordToken = null,
                ResetPasswordTokenExpiry = null
            };

            // Add the user to the in-memory database
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(); // Save changes to persist the user

            var newHashedToken = "newHashedToken";

            // Act
            Log.Information("Updating hashed user name password for user ID: {UserId}", userId);
            var result = await _passwordRepository.UpdateHashedUserNamePassword(userId, newHashedToken);

            // Assert
            Assert.Equal(newHashedToken, result); // Check the return value
            var updatedUser = await _dbContext.Users.FindAsync(userId);
            Assert.Equal(newHashedToken, updatedUser.ResetPasswordToken); // Check the updated token
            Assert.NotNull(updatedUser.ResetPasswordTokenExpiry); // Ensure timestamp is set

            Log.Information("Successfully updated hashed user name password for user ID: {UserId}", userId);
        }

        [Fact]
        public async Task UpdateHashedUserNamePassword_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 999ul; // Use a user ID that doesn't exist
            Log.Information("Testing UpdateHashedUserNamePassword for non-existent user ID: {UserId}", userId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _passwordRepository.UpdateHashedUserNamePassword(userId, "someToken"));

            Assert.Equal($"User with ID {userId} not found.", exception.Message);
            Log.Warning("Expected exception thrown for user ID {UserId}: {Message}", userId, exception.Message);
        }


        //-----------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task GetUserNamePasswordFromEmailId_ShouldReturnUserCredentials_WhenEmailExists()
        {
            // Arrange
            var email = "testuser@example.com";
            var user = new SecMan.Data.SQLCipher.User
            {
                Id = 1,
                UserName = "testuser",
                Password = "hashedPassword",
                ResetPasswordToken = "someHashedToken",
                ResetPasswordTokenExpiry = DateTime.UtcNow,
                Email = email
            };

            // Add the user to the in-memory database
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            Log.Information("Testing GetUserNamePasswordFromEmailId for email: {Email}", email);

            // Act
            var result = await _passwordRepository.GetUserNamePasswordFromEmailId(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.UserName, result.userName);
            Assert.Equal(user.Password, result.password);
            Assert.Equal(user.ResetPasswordToken, result.hashedUserNamePassword);
            Assert.Equal(user.ResetPasswordTokenExpiry, result.hashedUserNamePasswordTime);

            Log.Information("User credentials retrieved for email {Email}: {@Result}", email, result);
        }

        [Fact]
        public async Task GetUserNamePasswordFromEmailId_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            // Arrange
            var email = "nonexistentuser@example.com";
            Log.Information("Testing GetUserNamePasswordFromEmailId for non-existent email: {Email}", email);

            // Act
            var result = await _passwordRepository.GetUserNamePasswordFromEmailId(email);

            // Assert
            Assert.Null(result);
            Log.Information("Result for email {Email}: {Result}", email, result);
        }

        [Fact]
        public async Task GetUserNamePasswordFromEmailId_ShouldThrowArgumentException_WhenEmailIsNull()
        {
            // Arrange
            string email = null;
            Log.Information("Testing GetUserNamePasswordFromEmailId with null email.");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordRepository.GetUserNamePasswordFromEmailId(email));

            Assert.Equal("Email is required. (Parameter 'email')", exception.Message);
            Log.Warning("Expected exception thrown for null email: {Message}", exception.Message);
        }

        [Fact]
        public async Task GetUserNamePasswordFromEmailId_ShouldThrowArgumentException_WhenEmailIsEmpty()
        {
            // Arrange
            string email = "";
            Log.Information("Testing GetUserNamePasswordFromEmailId with empty email.");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordRepository.GetUserNamePasswordFromEmailId(email));

            Assert.Equal("Email is required. (Parameter 'email')", exception.Message);
            Log.Warning("Expected exception thrown for empty email: {Message}", exception.Message);
        }


        //------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task GetPasswordExpiryWarningValue_ShouldReturnValue_WhenNameExists()
        {
            // Arrange
            var name = "PasswordExpiryWarning";
            var sysFeatProp = new SysFeatProp
            {
                Name = name,
                Val = "WarningValue"
            };

            // Add the SysFeatProp to the in-memory database
            _dbContext.SysFeatProps.Add(sysFeatProp);
            await _dbContext.SaveChangesAsync();

            Log.Information("Testing GetPasswordExpiryWarningValue for name: {Name}", name);

            // Act
            var result = await _passwordRepository.GetPasswordExpiryWarningValue(name);

            // Assert
            Assert.Equal(sysFeatProp.Val, result);
            Log.Information("Expected value for name {Name}: {ExpectedValue}, Actual value: {ActualValue}", name, sysFeatProp.Val, result);
        }

        [Fact]
        public async Task GetPasswordExpiryWarningValue_ShouldReturnEmpty_WhenNameDoesNotExist()
        {
            // Arrange
            var name = "NonExistentName";
            Log.Information("Testing GetPasswordExpiryWarningValue for non-existent name: {Name}", name);

            // Act
            var result = await _passwordRepository.GetPasswordExpiryWarningValue(name);

            // Assert
            Assert.Equal(string.Empty, result);
            Log.Information("Result for name {Name}: {Result}", name, result);
        }

        [Fact]
        public async Task GetPasswordExpiryWarningValue_ShouldThrowArgumentException_WhenNameIsEmpty()
        {
            // Arrange
            string name = "";
            Log.Information("Testing GetPasswordExpiryWarningValue with empty name.");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordRepository.GetPasswordExpiryWarningValue(name));

            Assert.Equal("Name is required.", exception.Message);
            Log.Warning("Expected exception thrown for empty name: {Message}", exception.Message);
        }


        //[Fact]
        //public async Task GetPasswordExpiryWarningValue_ShouldThrowArgumentException_WhenNameIsNull()
        //{
        //    // Arrange
        //    string name = null; // Set to null to trigger the exception

        //    // Act & Assert
        //    var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
        //        _passwordRepository.GetPasswordExpiryWarningValue(name));

        //    Assert.Equal("Name is required.", exception.Message);
        //}








        //-------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task CheckForHashedToken_ShouldReturnUserId_WhenTokenExists()
        {
            // Arrange
            var hashedToken = "validHashedToken";
            var user = new SecMan.Data.SQLCipher.User
            {
                Id = 1,
                ResetPasswordToken = hashedToken
            };

            var options = new DbContextOptionsBuilder<Db>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            using (var context = new Db(options, string.Empty))
            {
                context.Database.OpenConnection();
                context.Database.EnsureCreated();
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var passwordRepository = new PasswordRepository(context);

                Log.Information("Testing CheckForHashedToken for token: {HashedToken}", hashedToken);

                // Act
                var result = await passwordRepository.CheckForHashedToken(hashedToken);

                // Assert
                Assert.Equal(user.Id, result);
                Log.Information("Expected user ID for token {HashedToken}: {ExpectedId}, Actual ID: {ActualId}", hashedToken, user.Id, result);
            }
        }



        [Fact]
        public async Task CheckForHashedToken_ShouldReturnZero_WhenTokenDoesNotExist()
        {
            // Arrange
            var hashedToken = "nonExistentToken";
            Log.Information("Testing CheckForHashedToken for non-existent token: {HashedToken}", hashedToken);

            // Act
            var result = await _passwordRepository.CheckForHashedToken(hashedToken);

            // Assert
            Assert.Equal(0UL, result); // Ensure the value is treated as a ulong
            Log.Information("Result for hashed token {HashedToken}: {Result}", hashedToken, result);
        }

        [Fact]
        public async Task CheckForHashedToken_ShouldThrowArgumentException_WhenTokenIsNull()
        {
            // Arrange
            string hashedToken = null;
            Log.Information("Testing CheckForHashedToken with null token.");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordRepository.CheckForHashedToken(hashedToken));

            Assert.Equal("Hashed token is required.", exception.Message);
            Log.Warning("Expected exception thrown for null token: {Message}", exception.Message);
        }

        [Fact]
        public async Task CheckForHashedToken_ShouldThrowArgumentException_WhenTokenIsEmpty()
        {
            // Arrange
            string hashedToken = "";
            Log.Information("Testing CheckForHashedToken with empty token.");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordRepository.CheckForHashedToken(hashedToken));

            Assert.Equal("Hashed token is required.", exception.Message);
            Log.Warning("Expected exception thrown for empty token: {Message}", exception.Message);
        }


        //------------------------------------------------------------------------------------------------

        [Fact]
        public async Task CheckForHashedTokenWithUserDetails_ShouldReturnUserCredentialsDto_WhenTokenIsValid()
        {
            // Arrange
            var hashedToken = "validToken";
            var user = new SecMan.Data.SQLCipher.User
            {
                Id = 1,
                UserName = "testuser",
                Password = "hashedPassword",
                ResetPasswordToken = hashedToken
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            Log.Information("Testing CheckForHashedTokenWithUserDetails for token: {HashedToken}", hashedToken);

            // Act
            var result = await _passwordRepository.CheckForHashedTokenWithUserDetails(hashedToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.userId);
            Assert.Equal(user.Password, result.Password);

            Log.Information("Result for token {HashedToken}: UserId: {UserId}, Password: {Password}", hashedToken, result.userId, result.Password);
        }

        [Fact]
        public async Task CheckForHashedTokenWithUserDetails_ShouldReturnNull_WhenTokenDoesNotExist()
        {
            // Arrange
            var hashedToken = "nonExistentToken";
            Log.Information("Testing CheckForHashedTokenWithUserDetails for non-existent token: {HashedToken}", hashedToken);

            // Act
            var result = await _passwordRepository.CheckForHashedTokenWithUserDetails(hashedToken);

            // Assert
            Assert.Null(result);
            Log.Information("Result for hashed token {HashedToken}: {Result}", hashedToken, result);
        }

        [Fact]
        public async Task CheckForHashedTokenWithUserDetails_ShouldThrowArgumentException_WhenTokenIsNull()
        {
            // Arrange
            string hashedToken = null;
            Log.Information("Testing CheckForHashedTokenWithUserDetails with null token.");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordRepository.CheckForHashedTokenWithUserDetails(hashedToken));

            Assert.Equal("Hashed token is required. (Parameter 'hashedToken')", exception.Message);
            Log.Warning("Expected exception thrown for null token: {Message}", exception.Message);
        }

        [Fact]
        public async Task CheckForHashedTokenWithUserDetails_ShouldThrowArgumentException_WhenTokenIsEmpty()
        {
            // Arrange
            string hashedToken = "";
            Log.Information("Testing CheckForHashedTokenWithUserDetails with empty token.");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordRepository.CheckForHashedTokenWithUserDetails(hashedToken));

            Assert.Equal("Hashed token is required. (Parameter 'hashedToken')", exception.Message);
            Log.Warning("Expected exception thrown for empty token: {Message}", exception.Message);
        }

    }
}
