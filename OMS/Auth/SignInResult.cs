namespace OMS.Auth
{
    public class SignInResult
    {
        private static readonly SignInResult _success = new SignInResult { Succeeded = true };
        private static readonly SignInResult _failed = new SignInResult();
        private static readonly SignInResult _notAllowed = new SignInResult { IsNotAllowed = true };

        public bool Succeeded { get; protected set; }
        public bool IsNotAllowed { get; protected set; }
        public static SignInResult Success => _success;
        public static SignInResult Failed => _failed;
        public static SignInResult NotAllowed => _notAllowed;

        public override string ToString()
        {
            return IsNotAllowed ? "NotAllowed" :
                Succeeded ? "Succeeded" : "Failed";
        }
    }
}