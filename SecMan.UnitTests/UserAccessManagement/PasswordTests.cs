using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SecMan.Interfaces.BL;
using SecMan.Model;
using Serilog;
using System.Threading.Tasks;
using UserAccessManagement.Controllers;
using Xunit;

namespace SecMan.UnitTests.UserAccessManagement
{
    public class PasswordTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IPasswordBl> _mockPasswordBl;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IAuthBL> _mockAuthBL;
        private readonly AuthController _authController;

        public PasswordTests()
        {
            _fixture = new Fixture();
            _mockPasswordBl = new Mock<IPasswordBl>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockAuthBL = new Mock<IAuthBL>();
            _authController = new AuthController(
                _mockPasswordBl.Object,
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockHttpContextAccessor.Object,
                _mockAuthBL.Object
            );
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            Log.Information("Starting test: ChangePassword_ShouldReturnBadRequest_WhenModelStateIsInvalid");

            // Arrange
            var changePasswordDto = new ChangePasswordDto
            {
                userName = "testuser",
                oldPassword = "oldpassword",
                newPassword = "short" // Invalid password (less than 6 characters)
            };

            // Simulate invalid model state
            _authController.ModelState.AddModelError("newPassword", "The NewPassword field is invalid."); // Adding a specific error

            // Act
            IActionResult result = await _authController.ChangePassword(changePasswordDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);

            // Check for ModelState errors
            var modelStateErrors = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(modelStateErrors.ContainsKey("newPassword")); // Verify the error is for newPassword

            // Verify that UpdatePasswordAsync was not called
            _mockPasswordBl.Verify(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());

            Log.Information("Test completed successfully: Model state is invalid as expected.");
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnBadRequest_WhenChangePasswordDtoIsNull()
        {
            Log.Information("Starting test: ChangePassword_ShouldReturnBadRequest_WhenChangePasswordDtoIsNull");

            // Act
            IActionResult result = await _authController.ChangePassword(null);
            Log.Information("Test result: {@Result}", result);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request.", badRequestResult.Value);
            _mockPasswordBl.Verify(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());

            Log.Information("Test completed successfully: ChangePasswordDto is null as expected.");
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnNoContent_WhenPasswordIsChangedSuccessfully()
        {
            Log.Information("Starting test: ChangePassword_ShouldReturnNoContent_WhenPasswordIsChangedSuccessfully");

            // Arrange
            var changePasswordDto = new ChangePasswordDto
            {
                userName = "testuser",
                oldPassword = "oldpassword",
                newPassword = "Valid1!" // Valid password
            };

            _mockPasswordBl.Setup(x => x.UpdatePasswordAsync(changePasswordDto.userName, changePasswordDto.oldPassword, changePasswordDto.newPassword))
                .ReturnsAsync("Password updated successfully."); // Simulate successful password change

            // Act
            IActionResult result = await _authController.ChangePassword(changePasswordDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockPasswordBl.Verify(x => x.UpdatePasswordAsync(changePasswordDto.userName, changePasswordDto.oldPassword, changePasswordDto.newPassword), Times.Once());

            Log.Information("Test completed successfully: Password changed as expected.");
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnBadRequest_WhenPasswordChangeFails()
        {
            Log.Information("Starting test: ChangePassword_ShouldReturnBadRequest_WhenPasswordChangeFails");

            // Arrange
            var changePasswordDto = new ChangePasswordDto
            {
                userName = "testuser",
                oldPassword = "oldpassword",
                newPassword = "Valid1!" // Valid password
            };

            _mockPasswordBl.Setup(x => x.UpdatePasswordAsync(changePasswordDto.userName, changePasswordDto.oldPassword, changePasswordDto.newPassword))
                .ReturnsAsync((string)null); // Simulate failed password change

            // Act
            IActionResult result = await _authController.ChangePassword(changePasswordDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Password change failed.", badRequestResult.Value);
            _mockPasswordBl.Verify(x => x.UpdatePasswordAsync(changePasswordDto.userName, changePasswordDto.oldPassword, changePasswordDto.newPassword), Times.Once());

            Log.Information("Test completed successfully: Password change failed as expected.");
        }

        [Fact]
        public async Task ForgotPassword_ShouldReturnNoContent_WhenLinkIsGenerated()
        {
            Log.Information("Starting test: ForgotPassword_ShouldReturnNoContent_WhenLinkIsGenerated");

            // Arrange
            var forgetPasswordDto = new ForgetPasswordDto
            {
                userName = "testuser"
            };

            var response = new GetForgetPasswordDto
            {
                userId = 1,
                domain = "http://example.com",
                userName = "testuser",
                password = "password123",
                emailId = "testuser@example.com",
                link = "http://example.com/reset-password"
            }; // Simulate a valid response with a link

            _mockPasswordBl.Setup(x => x.ForgetPasswordAsync(forgetPasswordDto.userName))
                .ReturnsAsync(response);

            // Act
            IActionResult result = await _authController.ForgotPassword(forgetPasswordDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockPasswordBl.Verify(x => x.ForgetPasswordAsync(forgetPasswordDto.userName), Times.Once());

            Log.Information("Test completed successfully: Password reset link generated as expected.");
        }


        [Fact]
        public async Task ForgotPassword_ShouldReturnBadRequest_WhenLinkGenerationFails()
        {
            Log.Information("Starting test: ForgotPassword_ShouldReturnBadRequest_WhenLinkGenerationFails");

            // Arrange
            var forgetPasswordDto = new ForgetPasswordDto
            {
                userName = "testuser"
            };

            _mockPasswordBl.Setup(x => x.ForgetPasswordAsync(forgetPasswordDto.userName))
                .ReturnsAsync((GetForgetPasswordDto)null); // Simulate a failure in generating the link

            // Act
            IActionResult result = await _authController.ForgotPassword(forgetPasswordDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to generate password reset link.", badRequestResult.Value);
            _mockPasswordBl.Verify(x => x.ForgetPasswordAsync(forgetPasswordDto.userName), Times.Once());

            Log.Information("Test completed successfully: Link generation failed as expected.");
        }

        [Fact]
        public async Task ForgotPassword_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            Log.Information("Starting test: ForgotPassword_ShouldReturnInternalServerError_WhenExceptionIsThrown");

            // Arrange
            var forgetPasswordDto = new ForgetPasswordDto
            {
                userName = "testuser"
            };

            _mockPasswordBl.Setup(x => x.ForgetPasswordAsync(forgetPasswordDto.userName))
                .ThrowsAsync(new Exception("Some error occurred")); // Simulate an exception

            // Act
            IActionResult result = await _authController.ForgotPassword(forgetPasswordDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error. Please try again later.", statusCodeResult.Value);
            _mockPasswordBl.Verify(x => x.ForgetPasswordAsync(forgetPasswordDto.userName), Times.Once());

            Log.Information("Test completed successfully: Exception handled as expected.");
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenTokenIsNull()
        {
            Log.Information("Starting test: ResetPassword_ShouldReturnBadRequest_WhenTokenIsNull");

            // Arrange
            var email = "testuser@example.com";
            var token = ""; // Invalid token

            // Act
            IActionResult result = await _authController.ResetPassword(token, email);
            Log.Information("Test result: {@Result}", result);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Token is required.", badRequestResult.Value);

            Log.Information("Test completed successfully: Token is null as expected.");
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenEmailIsNull()
        {
            Log.Information("Starting test: ResetPassword_ShouldReturnBadRequest_WhenEmailIsNull");

            // Arrange
            var email = ""; // Invalid email
            var token = "validToken";

            // Act
            IActionResult result = await _authController.ResetPassword(token, email);
            Log.Information("Test result: {@Result}", result);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email is required.", badRequestResult.Value);

            Log.Information("Test completed successfully: Email is null as expected.");
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenResetPasswordDtoIsNull()
        {
            Log.Information("Starting test: ResetPassword_ShouldReturnBadRequest_WhenResetPasswordDtoIsNull");

            // Arrange
            ResetPasswordDto resetPasswordDto = null;
            var authorizationHeader = "Bearer validToken";

            // Act
            IActionResult result = await _authController.ResetPassword(resetPasswordDto, authorizationHeader);
            Log.Information("Test result: {@Result}", result);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request.", badRequestResult.Value);

            Log.Information("Test completed successfully: ResetPasswordDto is null as expected.");
        }


        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenNewPasswordIsEmpty()
        {
            Log.Information("Starting test: ResetPassword_ShouldReturnBadRequest_WhenNewPasswordIsEmpty");

            // Arrange
            var resetPasswordDto = new ResetPasswordDto
            {
                newPassword = "" // Empty password
            };
            var authorizationHeader = "Bearer validToken";

            // Act
            IActionResult result = await _authController.ResetPassword(resetPasswordDto, authorizationHeader);
            Log.Information("Test result: {@Result}", result);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("New password is required.", badRequestResult.Value);

            Log.Information("Test completed successfully: New password is empty as expected.");
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnOk_WhenPasswordResetIsSuccessful()
        {
            Log.Information("Starting test: ResetPassword_ShouldReturnOk_WhenPasswordResetIsSuccessful");

            // Arrange
            var resetPasswordDto = new ResetPasswordDto
            {
                newPassword = "ValidPassword123!" // Valid password
            };
            var authorizationHeader = "Bearer validToken";

            _mockPasswordBl.Setup(x => x.CheckForHashedToken(authorizationHeader, resetPasswordDto.newPassword))
                .ReturnsAsync(true); // Simulate successful password reset

            // Act
            IActionResult result = await _authController.ResetPassword(resetPasswordDto, authorizationHeader);
            Log.Information("Test result: {@Result}", result);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Password reset successfully.", okResult.Value);

            Log.Information("Test completed successfully: Password reset successful as expected.");
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenTokenIsInvalid()
        {
            Log.Information("Starting test: ResetPassword_ShouldReturnBadRequest_WhenTokenIsInvalid");

            // Arrange
            var resetPasswordDto = new ResetPasswordDto
            {
                newPassword = "ValidPassword123!" // Valid password
            };
            var authorizationHeader = "Bearer invalidToken";

            _mockPasswordBl.Setup(x => x.CheckForHashedToken(authorizationHeader, resetPasswordDto.newPassword))
                .ReturnsAsync(false); // Simulate failed password reset

            // Act
            IActionResult result = await _authController.ResetPassword(resetPasswordDto, authorizationHeader);
            Log.Information("Test result: {@Result}", result);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid token or password reset failed.", badRequestResult.Value);

            Log.Information("Test completed successfully: Token is invalid as expected.");
        }



    }
}
