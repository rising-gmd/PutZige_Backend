namespace PutZige.Application.Common.Messages
{
    /// <summary>
    /// Centralized messages for rate limiting and throttling responses.
    /// </summary>
    public static class RateLimitMessages
    {
        /// <summary>
        /// User-facing message returned when a request is rejected due to rate limiting.
        /// </summary>
        public const string RateLimitExceeded = "Rate limit exceeded. Please try again later.";

        /// <summary>
        /// Message indicating rate limiting has been disabled by configuration or validation failure.
        /// </summary>
        public const string RateLimitDisabled = "Rate limiting is disabled.";

        /// <summary>
        /// Message indicating configuration validation failure for rate limiting.
        /// </summary>
        public const string RateLimitValidationFailed = "Rate limit configuration invalid; rate limiting disabled.";
    }
}
