namespace PutZige.Application.Common.Constants
{
    /// <summary>
    /// Application-wide constants. Never change these without version migration.
    /// </summary>
    public static class AppConstants
    {
        public static class Validation
        {
            public const int MinUsernameLength = 3;
            public const int MaxUsernameLength = 30;
            public const int MinDisplayNameLength = 2;
            public const int MaxDisplayNameLength = 50;
            public const int MinPasswordLength = 8;
            public const int MaxEmailLength = 255;

            public const string UsernameRegexPattern = "^[a-zA-Z0-9_]+$";
            public const string PasswordUppercaseRegex = "[A-Z]";
            public const string PasswordLowercaseRegex = "[a-z]";
            public const string PasswordDigitRegex = "[0-9]";
            public const string PasswordSpecialCharRegex = "[!@#$%^&*()_+\\-=\\[\\]{}|;:,.<>?]";
        }

        public static class Security
        {
            public const int BcryptWorkFactor = 12; // BCrypt cost parameter
            public const int EmailVerificationTokenExpirationDays = 1;
            public const int PasswordResetTokenExpirationHours = 1;
        }

        public static class Http
        {
            public const string ApiVersion = "v1";
            public const string ApiBaseRoute = "api/" + ApiVersion;
        }

        public static class Database
        {
            public const int DefaultCommandTimeoutSeconds = 30;
            public const int MaxRetryAttempts = 3;
        }
    }
}
