namespace OMS.Auth
{
    public class AuthErrorDescriber
    {
        public virtual AuthError DefaultError()
        {
            return new AuthError
            {
                Code = nameof(DefaultError),
                Description = "An unknown error has occured"
            };
        }

        public virtual AuthError ConcurrencyFailure()
        {
            return new AuthError
            {
                Code = nameof(ConcurrencyFailure),
                Description = "Concurrency error!"
            };
        }

        public virtual AuthError PasswordMismath()
        {
            return new AuthError
            {
                Code = nameof(PasswordMismath),
                Description = "Passwords didn't match!"
            };
        }

        public virtual AuthError InvalidToken()
        {
            return new AuthError
            {
                Code = nameof(InvalidToken),
                Description = "Invalid token provided!"
            };
        }

        public virtual AuthError LoginAlreadyAssociated()
        {
            return new AuthError
            {
                Code = nameof(LoginAlreadyAssociated),
                Description = "Login provided already associated with user!"
            };
        }

        public virtual AuthError InvalidUserName(string username)
        {
            return new AuthError
            {
                Code = nameof(InvalidUserName),
                Description = "Username: " + username + " is invalid!"
            };
        }

        public virtual AuthError DuplicateUserName(string username)
        {
            return new AuthError
            {
                Code = nameof(DuplicateUserName),
                Description = "Username: " + username + " is already taken!"
            };
        }

        public virtual AuthError InvalidEmail(string email)
        {
            return new AuthError
            {
                Code = nameof(InvalidEmail),
                Description = "Email: " + email + " is invalid!"
            };
        }

        public virtual AuthError DuplicateEmail(string email)
        {
            return new AuthError
            {
                Code = nameof(DuplicateEmail),
                Description = "Email: " + email + " is already taken!"
            };
        }

        public virtual AuthError PasswordTooShort(int minLength)
        {
            return new AuthError
            {
                Code = nameof(PasswordTooShort),
                Description = "Entered password is too short! It should be at least " + minLength.ToString() + " characters long."
            };
        }

        public virtual AuthError PasswordRequiresSymbol()
        {
            return new AuthError
            {
                Code = nameof(PasswordRequiresSymbol),
                Description = "Passwords require at least one symbol in them!"
            };
        }

        public virtual AuthError PasswordRequiresMixedCase()
        {
            return new AuthError
            {
                Code = nameof(PasswordRequiresMixedCase),
                Description = "Passwords require upper and lower case letters!"
            };
        }

        public virtual AuthError PasswordRequiresNumber()
        {
            return new AuthError
            {
                Code = nameof(PasswordRequiresNumber),
                Description = "Passwords require at least one number!"
            };
        }

        public virtual AuthError PasswordRequiresUniqueChars(int numUniqueChars)
        {
            return new AuthError
            {
                Code = nameof(PasswordRequiresUniqueChars),
                Description = "Passwords require at least " + numUniqueChars.ToString() + " unique characters!"
            };
        }
    }
}