namespace SecMan.Model
{
    public static class ResponseConstants
    {
        public const string Success = "Success";
        public const string UserAlreadyExists = "User Already Exists";
        public const string UserDoesNotExists = "User Does Not Exists";
        public const string SomeOfTheRoleNotPresent = "Some Of The Roles Not Present";
        public const string InvalidPassword = "Invalid Password";


        public const string InvalidRequest = "Invalid Request";
        public const string Conflict = "Conflict";


        public const string InvalidSessionId = "Invalid Session Id";
        public const string AccountLocked = "Account Is Locked, Please reset the password";
        public const string AccountRetired = "User Retired, cant login";
        public const string PasswordExpired = "Password Is Expired, Please Reset and Login Again";

    }


    public static class ResponseHeaders
    {
        public const string TotalCount = "x-total-count";
        public const string SSOSessionId = "SSO_SESSION_ID";
    }


    public static class EncryptionClassConstants
    {
        public const string NullEncryptedString = "The encryption key is not properly configured. It cannot be empty.";
        public const string InvalidEncryptedStringFormat = "Invalid encrypted string format.";
        public const string InvalidHexStringFormat = "Hex string must have an even number of digits.";
        public const string InvalidHashStringFormat = "The hashed password format is invalid.";
    }
}
