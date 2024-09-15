namespace SecMan.Model
{
    public static class ResponseConstants
    {
        public const string Success = "Success";



        public const string UserAlreadyExists = "User Already Exists";
        public const string UserDoesNotExists = "User Does Not Exists";
        public const string SomeOfTheRoleNotPresent = "Some Of The Roles Not Present";


        public const string InvalidRequest = "Invalid Request";
        public const string Conflict = "Conflict";


    }


    public static class ResponseHeaders
    {
        public const string TotalCount = "x-total-count";
    }


    public static class EncryptionClassConstants
    {
        public const string InvalidEncryptedStringFormat = "Invalid encrypted string format.";
        public const string InvalidHexStringFormat = "Hex string must have an even number of digits.";
        public const string InvalidHashStringFormat = "The hashed password format is invalid.";

    }
}
