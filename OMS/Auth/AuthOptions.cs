namespace OMS.Auth
{
    public class AuthOptions
    {
        public ClaimsOptions Claims { get; set; } = new ClaimsOptions();
        public PasswordOptions Passwords { get; set; } = new PasswordOptions();
        public SignInOptions SignIn { get; set; } = new SignInOptions();
        public StoreOptions Stores { get; set; } = new StoreOptions();
        public UserOptions Users { get; set; } = new UserOptions();
    }
}