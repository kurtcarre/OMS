using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using OMS.Auth.Models;

namespace OMS.Auth.Services
{
    public enum SignInResult
    {
        Success,
        RehashNeeded,
        Failed
    }

    public class SignInManager
    {
        private readonly UserManager userManager;
        private readonly IHttpContextAccessor contextAccessor;
        private HttpContext _context;

        public SignInManager(UserManager manager, IHttpContextAccessor _accessor)
        {
            userManager = manager;
            contextAccessor = _accessor;
        }

        private HttpContext Context
        {
            get
            {
                HttpContext context = _context ?? contextAccessor?.HttpContext;
                if (context == null)
                    throw new InvalidOperationException("HttopContext cannot be null!");

                return context;
            }
            set
            {
                _context = value;
            }
        }

        public ClaimsPrincipal CreateUserPrincipal(User user)
        {
            ClaimsIdentity id = GenerateClaimsAsync(user);
            return new ClaimsPrincipal(id);
        }

        protected ClaimsIdentity GenerateClaimsAsync(User user)
        {
            ClaimsIdentity id = new ClaimsIdentity("OMS.Auth", "Username", "Role");
            id.AddClaim(new Claim("UserID", user.Id));
            id.AddClaim(new Claim("Username", user.UserName));
            id.AddClaim(new Claim("Email", user.Email));
            return id;
        }

        public async Task SignInAsync(User user)
        {
            ClaimsPrincipal principal = CreateUserPrincipal(user);

            await Context.SignInAsync("OMS.Auth", principal);
        }

        public async Task SignOutAsync()
        {
            await Context.SignOutAsync("OMS.Auth");
        }

        public async Task<SignInResult> PasswordSignInAsync(string username, string password)
        {
            User user = await userManager.FindUserByUsername(username);
            if (user == null)
                return SignInResult.Failed;

            bool attempt = await userManager.CheckPasswordAsync(user, password);

            if (attempt == true)
            {
                await SignInAsync(user);
                return SignInResult.Success;
            }

            return SignInResult.Failed;
        }

        public bool IsSignedIn(ClaimsPrincipal principal)
        {
            if (principal == null)
                return false;

            return principal.Identities != null && principal.Identities.Any(i => i.AuthenticationType == "OMS.Auth");
        }
    }
}