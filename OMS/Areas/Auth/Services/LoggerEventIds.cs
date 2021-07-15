using Microsoft.Extensions.Logging;

namespace OMS.Auth.Services
{
    internal static class LoggerEventIds
    {
        public static readonly EventId UserCreated = new EventId(0, "UserCreated");
        public static readonly EventId UserLoggedIn = new EventId(1, "UserLoggedIn");
        public static readonly EventId UserLoggedOut = new EventId(2, "UserLoggedOut");
    }
}