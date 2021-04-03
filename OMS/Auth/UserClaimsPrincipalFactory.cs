using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OMS.Auth
{
    public class UserClaimsPrincipalFactory<TUser> : IUserClaimsPrincipalFactory<TUser> where TUser : class
    {
        public UserClaimsPrincipalFactory(UserManager<TUser> userManager, IOptions<AuthOptions> optionsAccessor)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));

            if (optionsAccessor == null)
                throw new ArgumentNullException(nameof(optionsAccessor));

            UserManager = userManager;
            Options = optionsAccessor.Value;
        }

        public UserManager<TUser> UserManager { get; private set; }
        public AuthOptions Options { get; private set; }

        public virtual async Task<ClaimsPrincipal> CreateAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            ClaimsIdentity id = await GenerateClaimsAsync(user);
            return new ClaimsPrincipal(id);
        }

        protected virtual async Task<ClaimsIdentity> GenerateClaimsAsync(TUser user)
        {
            string userId = await UserManager.GetUserIdAsync(user);
            string username = await UserManager.GetUserNameAsync(user);
            ClaimsIdentity id = new ClaimsIdentity(AuthConstants.AuthSchema, Options.Claims.UserNameClaimType, Options.Claims.RoleClaimType);
            id.AddClaim(new Claim(Options.Claims.UserIdClaimType, userId));
            id.AddClaim(new Claim(Options.Claims.UserNameClaimType, username));
            if(UserManager.SupportsUserEmail)
            {
                string email = await UserManager.GetEmailAsync(user);
                if (!string.IsNullOrEmpty(email))
                    id.AddClaim(new Claim(Options.Claims.EmailClaimType, email));
            }
            if (UserManager.SupportsUserSecurityStamp)
                id.AddClaim(new Claim(Options.Claims.SecurityStampClaimType, await UserManager.GetSecurityStampAsync(user)));

            if (UserManager.SupportsUserClaim)
                id.AddClaims(await UserManager.GetClaimsAsync(user));

            return id;
        }
    }
}