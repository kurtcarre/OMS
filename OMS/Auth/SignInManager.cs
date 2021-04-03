using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OMS.Auth
{
    public class SignInManager<TUser> where  TUser : class
    {
        private const string LoginProviderKey = "LoginProvider";
        private const string XsrfKey = "XsrfId";

        public SignInManager(UserManager<TUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<TUser> claimsFactory, IOptions<AuthOptions> options,
            ILogger<SignInManager<TUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<TUser> confirmation)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));

            if (contextAccessor == null)
                throw new ArgumentNullException(nameof(contextAccessor));

            if (claimsFactory == null)
                throw new ArgumentNullException(nameof(claimsFactory));

            UserManager = userManager;
            _contextAccessor = contextAccessor;
            ClaimsFactory = claimsFactory;
            Options = options?.Value ?? new AuthOptions();
            Logger = logger;
            _schemes = schemes;
            _confirmation = confirmation;
        }

        private readonly IHttpContextAccessor _contextAccessor;
        private HttpContext _context;
        private IAuthenticationSchemeProvider _schemes;
        private IUserConfirmation<TUser> _confirmation;
        public virtual ILogger Logger { get; set; }
        public UserManager<TUser> UserManager { get; set; }
        public IUserClaimsPrincipalFactory<TUser> ClaimsFactory { get; set; }
        public AuthOptions Options { get; set; }

        public HttpContext Context
        {
            get
            {
                HttpContext context = _context ?? _contextAccessor?.HttpContext;
                if (context == null)
                    throw new InvalidOperationException("HttpContext can't be null!");

                return context;
            }
            set
            {
                _context = value;
            }
        }

        public virtual async Task<ClaimsPrincipal> CreateUserPrincipalAsync(TUser user) => await ClaimsFactory.CreateAsync(user);

        public virtual bool IsSignedIn(ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.Identities != null && principal.Identities.Any(i => i.AuthenticationType == AuthConstants.AuthSchema);
        }

        public virtual async Task<bool> CanSignInAsync(TUser user)
        {
            if(Options.SignIn.RequireConfirmedEmail && !(await UserManager.IsEmailConfirmedAsync(user)))
            {
                Logger.LogWarning(LoggerEventIds.UserCannotSignInWithoutConfirmedEmail, "User cannot sign in without a confirmed email!");
                return false;
            }
            if(Options.SignIn.RequireConfirmedAccount && !(await _confirmation.IsConfirmedAsync(UserManager, user)))
            {
                Logger.LogWarning(LoggerEventIds.UserCannotSignInWithoutConfirmedAccount, "User cannot sign in without a confirmed account!");
                return false;
            }
            return true;
        }

        public virtual async Task RefreshSignInAsync(TUser user)
        {
            AuthenticateResult auth = await Context.AuthenticateAsync(AuthConstants.AuthSchema);
            IList<Claim> claims = Array.Empty<Claim>();

            Claim authenticationMethod = auth?.Principal?.FindFirst(ClaimTypes.AuthenticationMethod);
            Claim amr = auth?.Principal?.FindFirst("amr");

            if(authenticationMethod != null || amr != null)
            {
                claims = new List<Claim>();
                if (authenticationMethod != null)
                    claims.Add(authenticationMethod);

                if (amr != null)
                    claims.Add(amr);
            }

            await SignInWithClaimsAsync(user, auth?.Properties, claims);
        }

        public virtual Task SignInAsync(TUser user, bool isPersistent)
            => SignInAsync(user, new AuthenticationProperties { IsPersistent = isPersistent });

        public virtual Task SignInAsync(TUser user, AuthenticationProperties authenticationProperties, string authenticationMethod = null)
        {
            IList<Claim> claims;
            if(authenticationMethod != null)
            {
                claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.AuthenticationMethod, authenticationMethod));
            }
            else
            {
                claims = new List<Claim>();
                claims.Add(new Claim("amr", "pwd"));
            }

            return SignInWithClaimsAsync(user, authenticationProperties, claims);
        }

        public virtual Task SignInWithClaimsAsync(TUser user, bool isPersistent, IEnumerable<Claim> additionalClaims)
            => SignInWithClaimsAsync(user, new AuthenticationProperties { IsPersistent = isPersistent }, additionalClaims);

        public virtual async Task SignInWithClaimsAsync(TUser user, AuthenticationProperties authenticationProperties, IEnumerable<Claim> additionalClaims)
        {
            ClaimsPrincipal userPrincipal = await CreateUserPrincipalAsync(user);
            foreach (Claim claim in additionalClaims)
                userPrincipal.Identities.First().AddClaim(claim);

            await Context.SignInAsync(AuthConstants.AuthSchema, userPrincipal, authenticationProperties ?? new AuthenticationProperties());
        }

        public virtual async Task SignOutAsync()
        {
            await Context.SignOutAsync(AuthConstants.AuthSchema);
        }

        public virtual async Task<TUser> ValidateSecurityStampAsync(ClaimsPrincipal principal)
        {
            if (principal == null)
                return null;

            TUser user = await UserManager.GetUserAsync(principal);
            if (await ValidateSecurityStampAsync(user, principal.FindFirstValue(Options.Claims.SecurityStampClaimType)))
                return user;

            Logger.LogDebug(LoggerEventIds.SecurityStampValidationFailed, "Failed to validate security stamp!");
            return null;
        }

        public virtual async Task<bool> ValidateSecurityStampAsync(TUser user, string securityStamp)
            => user != null &&
            (!UserManager.SupportsUserSecurityStamp || securityStamp == await UserManager.GetSecurityStampAsync(user));

        public virtual async Task<SignInResult> PasswordSignInAsync(TUser user, string password, bool isPersistent)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            SignInResult attempt = await CheckPasswordSignInAsync(user, password);
            return attempt.Succeeded ? await StartSignInAsync(user, isPersistent) : attempt;
        }

        public virtual async Task<SignInResult> PasswordSignInAsync(string username, string password, bool isPersistent)
        {
            TUser user = await UserManager.FindByNameAsync(username);
            if (user == null)
                return SignInResult.Failed;

            return await PasswordSignInAsync(user, password, isPersistent);
        }

        public virtual async Task<SignInResult> CheckPasswordSignInAsync(TUser user, string password)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            SignInResult error = await CanSignInAsync(user) ? SignInResult.Success : SignInResult.NotAllowed;
            if (error != SignInResult.Success)
                return error;

            if (await UserManager.CheckPasswordAsync(user, password))
                return SignInResult.Success;

            Logger.LogWarning(LoggerEventIds.InvalidPassword, "Incorrect password supplied!");
            return SignInResult.Failed;
        }

        protected virtual async Task<SignInResult> StartSignInAsync(TUser user, bool isPersistent)
        {
            await SignInAsync(user, isPersistent);
            return SignInResult.Success;
        }
    }
}