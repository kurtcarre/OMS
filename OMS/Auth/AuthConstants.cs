namespace OMS.Auth
{
    public class AuthConstants
    {
        private static readonly string CookiePrefix = "OMS";

        public static readonly string ApplicationSchema = CookiePrefix + ".Internal";

        public static readonly string AuthSchema = ApplicationSchema + ".Auth";
    }
}