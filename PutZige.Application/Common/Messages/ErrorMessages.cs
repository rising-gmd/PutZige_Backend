namespace PutZige.Application.Common.Messages
{
    /// <summary>
    /// Centralized error messages for user-facing responses.
    /// </summary>
    public static class ErrorMessages
    {
        public static class Authentication
        {
            public const string EmailAlreadyTaken = "This email address is already registered.";
            public const string UsernameAlreadyTaken = "This username is already taken. Please choose another.";
            public const string InvalidCredentials = "Invalid email or password.";
            public const string EmailNotVerified = "Please verify your email before logging in.";
            public const string AccountLocked = "Your account has been locked. Please contact support.";
            public const string AccountInactive = "Your account is inactive. Please contact support.";
            public const string InvalidRefreshToken = "Invalid or expired refresh token.";
        }

        public static class Validation
        {
            public const string ValidationFailed = "Validation failed. Please check your input.";
            public const string EmailRequired = "Email is required.";
            public const string EmailInvalidFormat = "Invalid email format.";
            public const string EmailTooLong = "Email cannot exceed 255 characters.";

            public const string UsernameRequired = "Username is required.";
            public const string UsernameInvalidLength = "Username must be between 3 and 30 characters.";
            public const string UsernameInvalidCharacters = "Username can only contain letters, numbers, and underscores.";

            public const string DisplayNameRequired = "Display name is required.";
            public const string DisplayNameInvalidLength = "Display name must be between 2 and 50 characters.";

            public const string PasswordRequired = "Password is required.";
            public const string PasswordTooShort = "Password must be at least 8 characters.";
            public const string PasswordMissingUppercase = "Password must contain at least one uppercase letter.";
            public const string PasswordMissingLowercase = "Password must contain at least one lowercase letter.";
            public const string PasswordMissingDigit = "Password must contain at least one digit.";
            public const string PasswordMissingSpecialChar = "Password must contain at least one special character.";

            public const string ConfirmPasswordRequired = "Password confirmation is required.";
            public const string PasswordsDoNotMatch = "Passwords do not match.";
        }

        public static class General
        {
            public const string InternalServerError = "An internal error occurred. Please try again later.";
            public const string ResourceNotFound = "The requested resource was not found.";
            public const string UnauthorizedAccess = "You are not authorized to perform this action.";
        }

        public static class RateLimit
        {
            /// <summary>
            /// User-facing message returned when a request is rejected due to rate limiting.
            /// </summary>
            public const string Exceeded = "Rate limit exceeded. Please try again later.";

            /// <summary>
            /// Message indicating rate limiting has been disabled by configuration or validation failure.
            /// </summary>
            public const string Disabled = "Rate limiting is disabled.";

            /// <summary>
            /// Message indicating configuration validation failure for rate limiting.
            /// </summary>
            public const string ValidationFailed = "Rate limit configuration invalid; rate limiting disabled.";
        }

        public static class Messaging
        {
            public const string ReceiverNotFound = "Receiver not found";
            public const string MessageTooLong = "Message exceeds maximum length";
            public const string MessageNotFound = "Message not found";
            public const string UnauthorizedAccess = "Unauthorized to access this message";
        }
    }
}
