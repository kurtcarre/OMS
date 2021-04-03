using Microsoft.Extensions.Logging;

namespace OMS.Auth
{
    internal static class LoggerEventIds
    {
        public static readonly EventId InvalidPassword = new EventId(0, "InvalidPassword");
        public static readonly EventId ChangePasswordFailed = new EventId(1, "ChangePasswordFailed");
        public static readonly EventId GetSecurityStampFailed = new EventId(2, "GetSecurityStampFailed");
        public static readonly EventId AddLoginFailed = new EventId(3, "AddLoginFailed");
        public static readonly EventId UserValidationFailure = new EventId(4, "UserValidationFailure");
        public static readonly EventId PasswordValidationFailure = new EventId(5, "PasswordValidationFailure");
        public static readonly EventId UserCannotSignInWithoutConfirmedEmail = new EventId(6, "UserCannotSignInWithoutConfirmedEmail");
        public static readonly EventId UserCannotSignInWithoutConfirmedAccount = new EventId(7, "UserCannotSignInWithoutConfirmedAccount");
        public static readonly EventId SecurityStampValidationFailed = new EventId(8, "SecurityStampValidationFailed");
        public static readonly EventId UserLoggedIn = new EventId(9, "UserLoggedIn");
        public static readonly EventId UserCreated = new EventId(10, "UserCreated");
        public static readonly EventId UserLoggedOut = new EventId(11, "UserLoggedOut");
    }
}