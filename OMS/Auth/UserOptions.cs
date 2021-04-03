namespace OMS.Auth
{
    public class UserOptions
    {
        public string AllowedUserNameCharacters { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
        public bool RequireUniqueEmail { get; set; } = true;
    }
}