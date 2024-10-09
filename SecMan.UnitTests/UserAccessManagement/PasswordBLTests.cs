using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecMan.BL;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Interfaces.DAL;
using SecMan.Model;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Net.Mail;
using Serilog;

namespace SecMan.UnitTests.UserAccessManagement
{
    public class PasswordBLTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IEncryptionDecryption> _mockEncryptionDecryption;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<PasswordBL>> _mockLogger;
        private readonly PasswordBL _passwordBL;

        public PasswordBLTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockEncryptionDecryption = new Mock<IEncryptionDecryption>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<PasswordBL>>();
            _passwordBL = new PasswordBL(_mockUnitOfWork.Object, _mockEncryptionDecryption.Object, _mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldThrowArgumentException_WhenUserNameIsNull()
        {
            // Arrange
            string userName = null;
            string oldPassword = "oldPassword";
            string newPassword = "NewPassword123!";

            Log.Information("Testing UpdatePasswordAsync with null userName.");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordBL.UpdatePasswordAsync(userName, oldPassword, newPassword));

            Assert.Equal("Username is required. (Parameter 'userName')", exception.Message);
            Log.Information("Expected exception thrown: {Message}", exception.Message);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldThrowArgumentException_WhenOldPasswordIsNull()
        {
            // Arrange
            string userName = "testuser";
            string oldPassword = null;
            string newPassword = "NewPassword123!";

            Log.Information("Testing UpdatePasswordAsync with null oldPassword for user: {UserName}", userName);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordBL.UpdatePasswordAsync(userName, oldPassword, newPassword));

            Assert.Equal("Old password is required. (Parameter 'oldPassword')", exception.Message);
            Log.Information("Expected exception thrown: {Message}", exception.Message);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldThrowArgumentException_WhenNewPasswordIsInvalid()
        {
            // Arrange
            string userName = "testuser";
            string oldPassword = "oldPassword";
            string newPassword = "short"; // Invalid password

            Log.Information("Testing UpdatePasswordAsync with invalid newPassword for user: {UserName}", userName);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordBL.UpdatePasswordAsync(userName, oldPassword, newPassword));

            Assert.Equal("New password must be at least 6 characters long. (Parameter 'newPassword')", exception.Message);
            Log.Information("Expected exception thrown: {Message}", exception.Message);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldThrowArgumentException_WhenUserDoesNotExist()
        {
            // Arrange
            string userName = "nonExistentUser";
            string oldPassword = "oldPassword";
            string newPassword = "NewPassword123!";

            Log.Information("Testing UpdatePasswordAsync for non-existent user: {UserName}", userName);

            _mockUnitOfWork.Setup(uow => uow.IPasswordRepository.GetUserCredentials(userName))
                .ReturnsAsync((UserCredentialsDto)null); // Simulate user not found

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordBL.UpdatePasswordAsync(userName, oldPassword, newPassword));

            Assert.Equal("Invalid username. (Parameter 'userName')", exception.Message);
            Log.Information("Expected exception thrown: {Message}", exception.Message);
        }


        //-------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task ForgetPasswordAsync_ShouldThrowArgumentException_WhenUserNameIsNull()
        {
            // Arrange
            string userName = null;

            Log.Information("Testing ForgetPasswordAsync with null userName.");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordBL.ForgetPasswordAsync(userName));

            Assert.Equal("Username is required. (Parameter 'userName')", exception.Message);
            Log.Information("Expected exception thrown: {Message}", exception.Message);
        }

        [Fact]
        public async Task ForgetPasswordAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            string userName = "nonexistentUser";
            Log.Information("Testing ForgetPasswordAsync for non-existent user: {UserName}", userName);

            _mockUnitOfWork.Setup(m => m.IPasswordRepository.ForgetPasswordCredentials(userName))
                .ReturnsAsync((GetForgetPasswordDto)null); // Simulate non-existing user

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _passwordBL.ForgetPasswordAsync(userName));

            Assert.Equal("User not found.", exception.Message);
            Log.Information("Expected exception thrown: {Message}", exception.Message);
        }

        //[Fact]
        //public async Task ForgetPasswordAsync_ShouldReturnResetLink_WhenUserExists()
        //{
        //    // Arrange
        //    string userName = "testuser";
        //    string expectedEmail = "testuser@example.com";
        //    string expectedToken = "hashedToken"; // Simulating a valid token

        //    var userCredentials = new GetForgetPasswordDto
        //    {
        //        userId = 1,
        //        emailId = expectedEmail,
        //        domain = "example.com",
        //        userName = userName
        //    };

        //    _mockUnitOfWork.Setup(m => m.IPasswordRepository.ForgetPasswordCredentials(userName))
        //        .ReturnsAsync(userCredentials); // Simulate successful retrieval of user credentials

        //    _mockUnitOfWork.Setup(m => m.IPasswordRepository.UpdateHashedUserNamePassword(userCredentials.userId, It.IsAny<string>()))
        //        .ReturnsAsync(expectedToken); // Simulate successful token update

        //    _mockConfiguration.Setup(m => m["ResetPassword:BaseURL"]).Returns("http://example.com/reset"); // Ensure this is set

        //    // Act
        //    var result = await _passwordBL.ForgetPasswordAsync(userName);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(userCredentials.userName, result.userName);
        //    Assert.Equal(expectedEmail, result.emailId);

        //    // Check if the link is constructed correctly
        //    Assert.StartsWith("http://example.com/reset?token=", result.link);
        //    Assert.Contains("email=testuser%40example.com", result.link);

        //    // Ensure the token is included in the link
        //    Assert.Contains($"token={Uri.EscapeDataString(expectedToken)}", result.link);
        //}

        [Fact]
        public async Task ForgetPasswordAsync_ShouldThrowInvalidOperationException_WhenUpdateHashedTokenFails()
        {
            // Arrange
            string userName = "testuser";
            var userCredentials = new GetForgetPasswordDto
            {
                userName = userName,
                emailId = "testuser@example.com",
                userId = 1,
                domain = "example.com"
            };

            Log.Information("Setting up test for ForgetPasswordAsync with userName: {UserName}", userName);

            _mockUnitOfWork.Setup(m => m.IPasswordRepository.ForgetPasswordCredentials(userName))
                .ReturnsAsync(userCredentials);
            _mockUnitOfWork.Setup(m => m.IPasswordRepository.UpdateHashedUserNamePassword(It.IsAny<ulong>(), It.IsAny<string>()))
                .ReturnsAsync((string)null); // Simulate failure to update hashed token

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _passwordBL.ForgetPasswordAsync(userName));

            Assert.Equal("Failed to update hashed token.", exception.Message);
            Log.Information("Expected exception thrown: {Message}", exception.Message);
        }

        //-----------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task GenerateHashedToken_ShouldReturnHashedToken_WhenCalled()
        {
            // Arrange
            string userNamePassword = "testuser:testpassword";
            string expectedHashedToken = "hashedToken";
            _mockEncryptionDecryption.Setup(m => m.EncryptPassword(userNamePassword, false)).Returns(expectedHashedToken);

            Log.Information("Setting up test for GenerateHashedToken with userNamePassword: {UserNamePassword}", userNamePassword);

            // Act
            var result = await _passwordBL.GenerateHashedToken(userNamePassword);

            // Assert
            Assert.Equal(expectedHashedToken, result);
            Log.Information("Generated hashed token: {HashedToken}", result);
        }


        [Fact]
        public async Task GenerateHashedToken_ShouldHandleNullInput()
        {
            // Arrange
            string userNamePassword = null;

            Log.Information("Testing GenerateHashedToken with null input.");

            // Act
            var result = await _passwordBL.GenerateHashedToken(userNamePassword);

            // Assert
            Assert.Null(result); // Expecting a null result since the method doesn't throw an exception
            Log.Information("Result was null as expected.");
        }

        //----------------------------------------------------------------------------------------------------------

        //[Fact]
        //public async Task SendPasswordResetEmailAsync_ShouldReturnTrue_WhenEmailIsSentSuccessfully()
        //{
        //    // Arrange
        //    string recipientEmail = "testuser@example.com";
        //    string resetLink = "https://example.com/reset?token=abc123";

        //    // Mocking configuration for email settings
        //    _mockConfiguration.Setup(m => m["EmailConfiguration:From"]).Returns("no-reply@example.com");
        //    _mockConfiguration.Setup(m => m["EmailConfiguration:Password"]).Returns("validPassword");
        //    _mockConfiguration.Setup(m => m["EmailConfiguration:MailServer"]).Returns("smtp.example.com");

        //    // Here you would ideally run this against a test SMTP server

        //    // Act
        //    var result = await _passwordBL.SendPasswordResetEmailAsync(recipientEmail, resetLink);

        //    // Assert
        //    Assert.True(result);
        //    _mockLogger.Verify(m => m.LogInformation(It.IsAny<string>(), recipientEmail), Times.Once);
        //}

        //[Fact]
        //public async Task SendPasswordResetEmailAsync_ShouldReturnFalse_WhenSmtpExceptionOccurs()
        //{
        //    // Arrange
        //    string recipientEmail = "testuser@example.com";
        //    string resetLink = "https://example.com/reset?token=abc123";
        //    _mockConfiguration.Setup(m => m["EmailConfiguration:From"]).Returns("no-reply@example.com");
        //    _mockConfiguration.Setup(m => m["EmailConfiguration:Password"]).Returns("password");
        //    _mockConfiguration.Setup(m => m["EmailConfiguration:MailServer"]).Returns("smtp.example.com");

        //    // Act
        //    var result = await _passwordBL.SendPasswordResetEmailAsync(recipientEmail, resetLink);

        //    // Since we can't mock SmtpClient, the method will try to send the email.
        //    // Expect this to return false if the SMTP server is misconfigured or down.

        //    // Assert
        //    Assert.False(result);
        //    _mockLogger.Verify(m => m.LogError(It.IsAny<SmtpException>(), It.IsAny<string>(), recipientEmail), Times.Once);
        //}

        //[Fact]
        //public async Task SendPasswordResetEmailAsync_ShouldReturnFalse_WhenUnexpectedExceptionOccurs()
        //{
        //    // Arrange
        //    string recipientEmail = "testuser@example.com";
        //    string resetLink = "https://example.com/reset?token=abc123";
        //    _mockConfiguration.Setup(m => m["EmailConfiguration:From"]).Returns("no-reply@example.com");
        //    _mockConfiguration.Setup(m => m["EmailConfiguration:Password"]).Returns("password");
        //    _mockConfiguration.Setup(m => m["EmailConfiguration:MailServer"]).Returns("smtp.example.com");

        //    // Act
        //    var result = await _passwordBL.SendPasswordResetEmailAsync(recipientEmail, resetLink);

        //    // Similar to above, this should reflect real behavior.
        //    // Assert
        //    Assert.False(result);
        //    _mockLogger.Verify(m => m.LogError(It.IsAny<Exception>(), It.IsAny<string>(), recipientEmail), Times.Once);
        //}




        //-------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task CheckForHashedToken_ShouldReturnTrue_WhenTokenIsValidAndPasswordIsUpdated()
        {
            // Arrange
            string token = "validToken";
            string newPassword = "NewPassword123!";
            var userCred = new UserCredentialsDto
            {
                userId = 1,
                Password = "hashedPassword$2" // Format for PBKDF2
            };

            _mockUnitOfWork.Setup(m => m.IPasswordRepository.CheckForHashedTokenWithUserDetails(token))
                .ReturnsAsync(userCred);
            _mockUnitOfWork.Setup(m => m.IPasswordRepository.GetRecentPasswordsAsync(userCred.userId))
                .ReturnsAsync(new List<string> { "OldPassword1", "OldPassword2", "OldPassword3" });
            _mockEncryptionDecryption.Setup(m => m.VerifyHashPassword(It.IsAny<string>(), newPassword))
                .Returns(false); // New password does not match recent
            _mockEncryptionDecryption.Setup(m => m.EncryptPassword(newPassword, false))
                .Returns("encryptedNewPassword");

            Log.Information("Setting up test for CheckForHashedToken with token: {Token} and newPassword: {NewPassword}", token, newPassword);

            // Act
            var result = await _passwordBL.CheckForHashedToken(token, newPassword);

            // Assert
            Assert.True(result);
            _mockUnitOfWork.Verify(m => m.IPasswordRepository.UpdatePasswordAsync(userCred.userId, "encryptedNewPassword"), Times.Once);
            Log.Information("Password updated successfully for userId: {UserId}", userCred.userId);
        }

        [Fact]
        public async Task CheckForHashedToken_ShouldThrowInvalidOperationException_WhenNewPasswordMatchesRecentPassword()
        {
            // Arrange
            string token = "validToken";
            string newPassword = "OldPassword1"; // Same as a recent password
            var userCred = new UserCredentialsDto
            {
                userId = 1,
                Password = "hashedPassword$2" // Format for PBKDF2
            };

            _mockUnitOfWork.Setup(m => m.IPasswordRepository.CheckForHashedTokenWithUserDetails(token))
                .ReturnsAsync(userCred);
            _mockUnitOfWork.Setup(m => m.IPasswordRepository.GetRecentPasswordsAsync(userCred.userId))
                .ReturnsAsync(new List<string> { "OldPassword1", "OldPassword2", "OldPassword3" });
            _mockEncryptionDecryption.Setup(m => m.VerifyHashPassword(It.IsAny<string>(), newPassword))
                .Returns(true); // New password matches a recent password

            Log.Information("Setting up test for CheckForHashedToken with token: {Token} and newPassword: {NewPassword}", token, newPassword);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _passwordBL.CheckForHashedToken(token, newPassword));

            Assert.Equal("The new password cannot be the same as any of the last three passwords. Please choose a different password.", exception.Message);
            Log.Warning("InvalidOperationException thrown for token: {Token} with message: {Message}", token, exception.Message);
        }

        //[Fact]
        //public async Task CheckForHashedToken_ShouldThrowArgumentException_WhenTokenIsNullOrEmpty()
        //{
        //    // Arrange
        //    string token = null;
        //    string newPassword = "NewPassword123!";

        //    // Act & Assert
        //    var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
        //        _passwordBL.CheckForHashedToken(token, newPassword));

        //    Assert.Equal("Token is required.", exception.Message);

        //    // Verify the logger was called with a warning message
        //    _mockLogger.Verify(m => m.LogWarning("Token is null or empty."), Times.Once);
        //}

        //[Fact]
        //public async Task CheckForHashedToken_ShouldThrowArgumentException_WhenNewPasswordIsNullOrEmpty()
        //{
        //    // Arrange
        //    string token = "validToken";
        //    string newPassword = null;

        //    // Act & Assert
        //    var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
        //        _passwordBL.CheckForHashedToken(token, newPassword));

        //    Assert.Equal("New password is required.", exception.Message);
        //    // Verify the logger was called with a warning message
        //    _mockLogger.Verify(m => m.LogWarning("New password is null or empty."), Times.Once);
        //}

        [Fact]
        public async Task CheckForHashedToken_ShouldThrowFormatException_WhenPasswordFormatIsInvalid()
        {
            // Arrange
            string token = "validToken";
            string newPassword = "NewPassword123!";
            var userCred = new UserCredentialsDto
            {
                userId = 1,
                Password = "invalidFormat" // Invalid password format
            };

            _mockUnitOfWork.Setup(m => m.IPasswordRepository.CheckForHashedTokenWithUserDetails(token))
                .ReturnsAsync(userCred);

            Log.Information("Setting up test for CheckForHashedToken with token: {Token} and newPassword: {NewPassword}", token, newPassword);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FormatException>(() =>
                _passwordBL.CheckForHashedToken(token, newPassword));

            Assert.Equal("Invalid password format.", exception.Message);
            Log.Warning("FormatException thrown for token: {Token} with message: {Message}", token, exception.Message);
        }

        //[Fact]
        //public async Task CheckForHashedToken_ShouldReturnFalse_WhenTokenIsInvalid()
        //{
        //    // Arrange
        //    string token = "invalidToken";
        //    string newPassword = "NewPassword123!";

        //    // Setup the mock to return null for an invalid token
        //    _mockUnitOfWork.Setup(m => m.IPasswordRepository.CheckForHashedTokenWithUserDetails(token))
        //        .ReturnsAsync((UserCredentialsDto)null);

        //    Log.Information("Testing CheckForHashedToken with invalid token: {Token}", token);

        //    // Act
        //    var result = await _passwordBL.CheckForHashedToken(token, newPassword);

        //    // Assert
        //    Assert.False(result);
        //    _mockLogger.Verify(m => m.LogWarning(It.IsAny<string>(), token), Times.Once); // Verify that LogWarning was called with any message
        //    Log.Information("CheckForHashedToken returned false for invalid token: {Token}", token);
        //}



    }
}
