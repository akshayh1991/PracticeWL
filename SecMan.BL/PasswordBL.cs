using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Interfaces.DAL;
using SecMan.Model;
using System.Globalization;
using System.Net;
using System.Net.Mail;

namespace SecMan.BL
{
    public class PasswordBL : IPasswordBl
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionDecryption _encryptionDecryption;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PasswordBL> _logger;

        public PasswordBL(IUnitOfWork unitOfWork, IEncryptionDecryption encryptionDecryption, IConfiguration configuration, ILogger<PasswordBL> logger)
        {
            _unitOfWork = unitOfWork;
            _encryptionDecryption = encryptionDecryption;
            _configuration = configuration;
            _logger = logger;
        }


        public async Task<string> UpdatePasswordAsync(string userName, string oldPassword, string newPassword)
        {
            _logger.LogInformation("UpdatePasswordAsync called for user: {Username}", userName);

            if (string.IsNullOrWhiteSpace(userName))
            {
                _logger.LogWarning("Username is null or empty.");
                throw new ArgumentException("Username is required.", nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                _logger.LogWarning("Old password is null or empty for user: {Username}", userName);
                throw new ArgumentException("Old password is required.", nameof(oldPassword));
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                _logger.LogWarning("New password is invalid for user: {Username}", userName);
                throw new ArgumentException("New password must be at least 6 characters long.", nameof(newPassword));
            }

            UserCredentialsDto? userCredentials = await _unitOfWork.IPasswordRepository.GetUserCredentials(userName);
            if (userCredentials == null)
            {
                _logger.LogWarning("User not found for username: {Username}", userName);
                throw new ArgumentException("Invalid username.", nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(userCredentials.Password))
            {
                _logger.LogError("Password is null or empty for user: {Username}", userName);
                throw new FormatException("Invalid password format.");
            }

            string[] parts = userCredentials.Password.Split('$');
            if (parts.Length < 2)
            {
                _logger.LogError("Invalid password format for user: {Username}", userName);
                throw new FormatException("Invalid password format.");
            }

            bool isUpdated;
            switch (parts[1])
            {
                case "2":
                    isUpdated = await UpdatePasswordWithPBKDF2(userCredentials, oldPassword, newPassword);
                    break;
                case "1":
                    isUpdated = await UpdatePasswordWithAES(userCredentials, oldPassword, newPassword);
                    break;
                default:
                    _logger.LogError("Unknown password format for user: {Username}", userName);
                    throw new NotSupportedException("Unsupported password format.");
            }

            if (isUpdated)
            {
                _logger.LogInformation("Password updated successfully for user: {Username}", userName);
                return "Password updated successfully.";
            }
            else
            {
                _logger.LogWarning("Password update failed for user: {Username}", userName);
                return "Password update failed.";
            }
        }
        private async Task<bool> UpdatePasswordWithPBKDF2(UserCredentialsDto userCredentials, string oldPassword, string newPassword)
        {
            _logger.LogInformation("UpdatePasswordWithPBKDF2 called for user: {UserId}", userCredentials?.userId);

            if (userCredentials == null)
            {
                _logger.LogWarning("UserCredentialsDto is null.");
                throw new ArgumentNullException(nameof(userCredentials), "User credentials cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                _logger.LogWarning("Old password is null or empty for user: {UserId}", userCredentials.userId);
                throw new ArgumentException("Old password is required.");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                _logger.LogWarning("New password is invalid for user: {UserId}", userCredentials.userId);
                throw new ArgumentException("New password must be at least 6 characters long.");
            }

            if (string.IsNullOrWhiteSpace(userCredentials.Password))
            {
                _logger.LogError("Password is null or empty for user: {UserId}", userCredentials.userId);
                throw new InvalidOperationException("User password cannot be null or empty.");
            }

            bool isValid = _encryptionDecryption.VerifyHashPassword(userCredentials.Password, oldPassword);

            if (!isValid)
            {
                _logger.LogWarning("Old password verification failed for user: {UserId}", userCredentials.userId);
                return false;
            }

            //-----------------------------------PasswordHistoryCheckingForPKDF2----------------------------------------------

            Task<List<string>> recentPasswords = _unitOfWork.IPasswordRepository.GetRecentPasswordsAsync(userCredentials.userId);
            bool existingPreviouly = false;
            foreach (var pass in recentPasswords.Result)
            {
                existingPreviouly = _encryptionDecryption.VerifyHashPassword(pass, newPassword);
                if (existingPreviouly)
                {
                    _logger.LogWarning("The new password matches one of the last three passwords.");
                    throw new InvalidOperationException("The new password cannot be the same as any of the last three passwords. Please choose a different password.");
                }
            }

            //--------------------------------------------------------------------------------------------------------

            string encNewPassword = _encryptionDecryption.EncryptPassword(newPassword, false);

            await _unitOfWork.IPasswordRepository.UpdatePasswordAsync(userCredentials.userId, encNewPassword.Trim());

            _logger.LogInformation("Password updated successfully for user: {UserId}", userCredentials.userId);
            return true;
        }
        private async Task<bool> UpdatePasswordWithAES(UserCredentialsDto userCredentials, string oldPassword, string newPassword)
        {
            _logger.LogInformation("UpdatePasswordWithAES called for user: {UserId}", userCredentials?.userId);

            if (userCredentials == null)
            {
                _logger.LogWarning("UserCredentialsDto is null.");
                throw new ArgumentNullException(nameof(userCredentials), "User credentials cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                _logger.LogWarning("Old password is null or empty for user: {UserId}", userCredentials.userId);
                throw new ArgumentException("Old password is required.");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                _logger.LogWarning("New password is invalid for user: {UserId}", userCredentials.userId);
                throw new ArgumentException("New password must be at least 6 characters long.");
            }

            if (string.IsNullOrWhiteSpace(userCredentials.Password))
            {
                _logger.LogError("Password is null or empty for user: {UserId}", userCredentials.userId);
                throw new InvalidOperationException("User password cannot be null or empty.");
            }

            //-----------------------------------PasswordHistoryCheckingForAES----------------------------------------------

            Task<List<string>> recentPasswords = _unitOfWork.IPasswordRepository.GetRecentPasswordsAsync(userCredentials.userId);
            foreach (var pass in recentPasswords.Result)
            {
                string existingPreviouly = _encryptionDecryption.DecryptPasswordAES256(pass);
                if (existingPreviouly == newPassword)
                {
                    _logger.LogWarning("The new password matches one of the last three passwords.");
                    throw new InvalidOperationException("The new password cannot be the same as any of the last three passwords. Please choose a different password.");
                }
            }

            //--------------------------------------------------------------------------------------------------------

            string decryptedPassword = _encryptionDecryption.DecryptPasswordAES256(userCredentials.Password);

            if (decryptedPassword == oldPassword)
            {
                string encryptedPassword = _encryptionDecryption.EncryptPassword(newPassword, true);
                await _unitOfWork.IPasswordRepository.UpdatePasswordAsync(userCredentials.userId, encryptedPassword.Trim());

                _logger.LogInformation("Password updated successfully for user: {UserId}", userCredentials.userId);
                return true;
            }

            _logger.LogWarning("Old password verification failed for user: {UserId}", userCredentials.userId);
            return false;
        }
        public async Task<GetForgetPasswordDto> ForgetPasswordAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("Username is required.", nameof(userName));
            }

            try
            {
                GetForgetPasswordDto? userCredentials = await _unitOfWork.IPasswordRepository.ForgetPasswordCredentials(userName);

                if (userCredentials == null)
                {
                    _logger.LogWarning("ForgetPasswordAsync called for non-existing user: {Username}", userName);
                    throw new KeyNotFoundException("User not found.");
                }

                string userCred = $"{userCredentials.domain}:{userCredentials.userName}:{userCredentials.userId}";
                string generatedHashToken = await GenerateHashedToken(userCred);

                var savedHashToken = await _unitOfWork.IPasswordRepository.UpdateHashedUserNamePassword(userCredentials.userId, generatedHashToken);
                if (savedHashToken == null)
                {
                    _logger.LogWarning("Failed to update hashed token for user: {Username}", userName);
                    throw new InvalidOperationException("Failed to update hashed token.");
                }

                string? baseURL = _configuration["ResetPassword:BaseURL"];
                string emailId = userCredentials.emailId ?? throw new InvalidOperationException("Email ID is required."); // Handle potential null

                string resetPasswordLink = $"{baseURL}?token={Uri.EscapeDataString(generatedHashToken)}&email={Uri.EscapeDataString(emailId)}";

                bool mailSent = await SendPasswordResetEmailAsync(emailId, resetPasswordLink);
                if (!mailSent)
                {
                    _logger.LogWarning("Failed to send password reset email to: {Email}", emailId);
                    throw new InvalidOperationException("Failed to send password reset email.");
                }

                var userCredentialsDto = new GetForgetPasswordDto
                {
                    userName = userCredentials.userName,
                    emailId = emailId,
                    link = resetPasswordLink
                };

                _logger.LogInformation("Password reset link generated successfully for user: {Username}", userName);
                return userCredentialsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the forget password request for user: {Username}", userName);
                throw;
            }
        }
        public Task<string> GenerateHashedToken(string userNamePassword)
        {
            string hashedToken = _encryptionDecryption.EncryptPassword(userNamePassword, false);
            return Task.FromResult(hashedToken);
        }
        public async Task<bool> SendPasswordResetEmailAsync(string recipientEmail, string resetLink)
        {
            string? fromEmail = _configuration["EmailConfiguration:From"];
            string? netPassword = _configuration["EmailConfiguration:Password"];
            string? smtpMailServer = _configuration["EmailConfiguration:MailServer"];
            int smtpPort = 587;

            using (var mailMessage = new MailMessage())
            {
                if (fromEmail != null)
                {
                    mailMessage.From = new MailAddress(fromEmail);
                }
                mailMessage.To.Add(new MailAddress(recipientEmail));
                mailMessage.Subject = "Password Reset Link";
                mailMessage.Body = $"<h2>To reset your password, please click the link below:</h2>" +
                          $"<a href='{resetLink}'>Reset Password</a>";
                mailMessage.IsBodyHtml = true;
                mailMessage.Priority = MailPriority.Normal;

                using (var smtpClient = new SmtpClient(smtpMailServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(fromEmail, netPassword);
                    smtpClient.EnableSsl = true;

                    try
                    {
                        await smtpClient.SendMailAsync(mailMessage);
                        _logger.LogInformation("Password reset email sent successfully to: {Email}", recipientEmail);
                        return true;
                    }
                    catch (SmtpException ex)
                    {
                        _logger.LogError(ex, "Failed to send password reset email to: {Email}", recipientEmail);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An unexpected error occurred while sending email to: {Email}", recipientEmail);
                        return false;
                    }
                }
            }
        }
        public async Task<bool> GetUserNamePasswordAsync(string email, string token)
        {
            _logger.LogInformation("GetUserNamePasswordAsync called for email: {Email}", email);

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Email is null or empty.");
                throw new ArgumentException("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token is null or empty.");
                throw new ArgumentException("Token is required.");
            }

            var getUserNamePasswordResponse = await _unitOfWork.IPasswordRepository.GetUserNamePasswordFromEmailId(email);

            if (getUserNamePasswordResponse == null)
            {
                _logger.LogWarning("No user found for email: {Email}", email);
                return false;
            }

            var timeDifference = DateTime.Now - getUserNamePasswordResponse.hashedUserNamePasswordTime;

            string? passwordExpiryValueString = await _unitOfWork.IPasswordRepository.GetPasswordExpiryWarningValue("Password Expiry Warning");
            if (string.IsNullOrEmpty(passwordExpiryValueString)) { passwordExpiryValueString = "1"; }

            if (!TimeSpan.TryParse(passwordExpiryValueString, CultureInfo.InvariantCulture, out TimeSpan passwordExpiryValue))
            {
                _logger.LogError("Invalid password expiry value: {ExpiryValue}", passwordExpiryValueString);
                throw new FormatException("Invalid password expiry value.");
            }

            if (timeDifference < passwordExpiryValue)
            {
                if (token == getUserNamePasswordResponse.hashedUserNamePassword)
                {
                    _logger.LogInformation("Token validated successfully for email: {Email}", email);
                    return true;
                }
                _logger.LogWarning("Token validation failed for email: {Email}", email);
                return false;
            }
            else
            {
                _logger.LogWarning("Token expired for email: {Email}. Time difference: {TimeDifference}, Expiry Value: {ExpiryValue}", email, timeDifference, passwordExpiryValue);
                return false;
            }
        }
        public async Task<bool> CheckForHashedToken(string token, string newPassword)
        {
            _logger.LogInformation("CheckForHashedToken called with token: {Token}", token);

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token is null or empty.");
                throw new ArgumentException("Token is required.");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                _logger.LogWarning("New password is null or empty.");
                throw new ArgumentException("New password is required.");
            }

            var userCred = await _unitOfWork.IPasswordRepository.CheckForHashedTokenWithUserDetails(token);
            if (userCred != null)
            {
                if (string.IsNullOrWhiteSpace(userCred.Password))
                {
                    _logger.LogWarning("Password is null or empty for userId: {UserId}", userCred.userId);
                    throw new FormatException("Invalid password format.");
                }

                string[] parts = userCred.Password.Split('$');
                if (parts.Length < 2)
                {
                    throw new FormatException("Invalid password format.");
                }

                if (parts[1] == "2")
                {
                    //-----------------------------------PasswordHistoryCheckingForPKDF2----------------------------------------------
                    Task<List<string>> recentPasswords = _unitOfWork.IPasswordRepository.GetRecentPasswordsAsync(userCred.userId);
                    bool existingPreviouly = false;
                    foreach (var pass in recentPasswords.Result)
                    {
                        existingPreviouly = _encryptionDecryption.VerifyHashPassword(pass, newPassword);
                        if (existingPreviouly)
                        {
                            _logger.LogWarning("The new password matches one of the last three passwords.");
                            throw new InvalidOperationException("The new password cannot be the same as any of the last three passwords. Please choose a different password.");
                        }
                    }

                    //--------------------------------------------------------------------------------------------------------

                    string encNewPasswordwithpkdf2 = _encryptionDecryption.EncryptPassword(newPassword, false);
                    await _unitOfWork.IPasswordRepository.UpdatePasswordAsync(userCred.userId, encNewPasswordwithpkdf2);
                    _logger.LogInformation("Password updated successfully for userId: {UserId}", userCred.userId);
                    return true;
                }
                else if (parts[1] == "1")
                {
                    //-----------------------------------PasswordHistoryCheckingForAES----------------------------------------------

                    Task<List<string>> recentPasswords = _unitOfWork.IPasswordRepository.GetRecentPasswordsAsync(userCred.userId);
                    foreach (var pass in recentPasswords.Result)
                    {
                        string existingPreviouly = _encryptionDecryption.DecryptPasswordAES256(pass);
                        if (existingPreviouly == newPassword)
                        {
                            _logger.LogWarning("The new password matches one of the last three passwords.");
                            throw new InvalidOperationException("The new password cannot be the same as any of the last three passwords. Please choose a different password.");
                        }
                    }

                    //--------------------------------------------------------------------------------------------------------
                    string encNewPasswordwithAes = _encryptionDecryption.EncryptPassword(newPassword, true);
                    await _unitOfWork.IPasswordRepository.UpdatePasswordAsync(userCred.userId, encNewPasswordwithAes);
                    _logger.LogInformation("Password updated successfully for userId: {UserId}", userCred.userId);
                    return true;
                }
            }
            _logger.LogWarning("Invalid token: {Token}", token);
            return false;
        }
    }
}
