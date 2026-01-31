#nullable enable
using System;

namespace PutZige.Application.Common
{
    public static class AppConstants
    {
        public static class Security
        {
            // Maximum allowed failed login attempts before locking an account
            public const int MaxLoginAttempts = 5;

            // Lockout duration in minutes
            public const int LockoutMinutes = 15;
        }

        public static class Messaging
        {
            public const int MaxMessageLength = 4000;
            public const int DefaultPageSize = 50;
            public const int MaxPageSize = 100;
        }
    }
}
