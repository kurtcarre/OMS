namespace OMS.Auth
{
    public class PasswordOptions
    {
        public int MinLength { get; set; } = 6;
        public int RequireUniqueChars { get; set; } = 2;
        public bool RequireSymbol { get; set; } = false;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireNumber { get; set; } = true;
    }
}