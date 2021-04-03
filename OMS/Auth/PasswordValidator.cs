using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.Auth
{
    public class PasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {
        public PasswordValidator(AuthErrorDescriber errors = null)
        {
            Describer = errors ?? new AuthErrorDescriber();
        }

        public AuthErrorDescriber Describer { get; private set; }

        public virtual Task<AuthResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            List<AuthError> errors = new List<AuthError>();
            PasswordOptions options = manager.Options.Passwords;
            if (string.IsNullOrWhiteSpace(password) || password.Length < options.MinLength)
                errors.Add(Describer.PasswordTooShort(options.MinLength));

            if (options.RequireSymbol && password.All(IsLetterOrNumber))
                errors.Add(Describer.PasswordRequiresSymbol());

            if (options.RequireNumber && !password.Any(IsNumber))
                errors.Add(Describer.PasswordRequiresNumber());

            if (options.RequireLowercase && !password.Any(IsLower))
                errors.Add(Describer.PasswordRequiresMixedCase());

            if (options.RequireUppercase && !password.Any(IsUpper))
                errors.Add(Describer.PasswordRequiresMixedCase());

            if (options.RequireUniqueChars >= 1 && password.Distinct().Count() < options.RequireUniqueChars)
                errors.Add(Describer.PasswordRequiresUniqueChars(options.RequireUniqueChars));

            return Task.FromResult(errors.Count == 0 ? AuthResult.Success : AuthResult.Failed(errors.ToArray()));
        }

        public virtual bool IsNumber(char c)
        {
            return c >= '0' && c <= '9';
        }

        public virtual bool IsLower(char c)
        {
            return c >= 'a' && c <= 'z';
        }

        public virtual bool IsUpper(char c)
        {
            return c >= 'A' && c <= 'Z';
        }

        public virtual bool IsLetterOrNumber(char c)
        {
            return IsUpper(c) || IsLower(c) || IsNumber(c);
        }
    }
}