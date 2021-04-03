using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.Auth
{
    public class UserValidator<TUser> : IUserValidator<TUser> where TUser : class
    {
        public UserValidator(AuthErrorDescriber errors = null)
        {
            Describer = errors ?? new AuthErrorDescriber();
        }

        public AuthErrorDescriber Describer { get; private set; }

        public virtual async Task<AuthResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            List<AuthError> errors = new List<AuthError>();
            await ValidateUserName(manager, user, errors);
            if (manager.Options.Users.RequireUniqueEmail)
                await ValidateEmail(manager, user, errors);

            return errors.Count > 0 ? AuthResult.Failed(errors.ToArray()) : AuthResult.Success;
        }

        private async Task ValidateUserName(UserManager<TUser> manager, TUser user, ICollection<AuthError> errors)
        {
            string username = await manager.GetUserNameAsync(user);
            if (string.IsNullOrWhiteSpace(username))
                errors.Add(Describer.InvalidUserName(username));
            else if (!string.IsNullOrEmpty(manager.Options.Users.AllowedUserNameCharacters) && username.Any(c => !manager.Options.Users.AllowedUserNameCharacters.Contains(c)))
                errors.Add(Describer.InvalidUserName(username));
            else
            {
                TUser owner = await manager.FindByNameAsync(username);
                if (owner != null && !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user)))
                    errors.Add(Describer.DuplicateUserName(username));
            }
        }

        private async Task ValidateEmail(UserManager<TUser> manager, TUser user, List<AuthError> errors)
        {
            string email = await manager.GetEmailAsync(user);
            if (string.IsNullOrWhiteSpace(email))
                errors.Add(Describer.InvalidEmail(email));
            else if (!new EmailAddressAttribute().IsValid(email))
                errors.Add(Describer.InvalidEmail(email));
            else
            {
                TUser owner = await manager.FindByEmailAsync(email);
                if (owner != null && !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user)) && manager.Options.Users.RequireUniqueEmail)
                    errors.Add(Describer.DuplicateEmail(email));
            }
        }
    }
}