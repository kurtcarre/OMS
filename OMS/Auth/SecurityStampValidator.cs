using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OMS.Auth
{
    public class SecurityStampValidator<TUser> : ISecurityStampValidator where TUser : class
    {
        public SecurityStampValidator(IOptions<SecurityStampValidatorOptions> options, SignInManager<TUser> signInManager, ISystemClock clock, ILoggerFactory logger)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (signInManager == null)
                throw new ArgumentNullException(nameof(signInManager));

            SignInManager = signInManager;
            Options = options.Value;
            Clock = clock;
            Logger = logger.CreateLogger(GetType().FullName);
        }

        public SignInManager<TUser> SignInManager { get; }
        public SecurityStampValidatorOptions Options { get; }
        public ISystemClock Clock { get; }
        public ILogger Logger { get; set; }

        protected virtual async Task SecurityStampVerified(TUser user, CookieValidatePrincipalContext context)
        {
            ClaimsPrincipal newPrincipal = await SignInManager.CreateUserPrincipalAsync(user);

            if(Options.OnRefreshingPrincipal != null)
            {
                SecurityStampRefreshingPrincipalContext replaceContext = new SecurityStampRefreshingPrincipalContext
                {
                    CurrentPrincipal = context.Principal,
                    NewPrincipal = newPrincipal
                };

                await Options.OnRefreshingPrincipal(replaceContext);
                newPrincipal = replaceContext.NewPrincipal;
            }

            context.ReplacePrincipal(newPrincipal);
            context.ShouldRenew = true;
        }

        protected virtual Task<TUser> VerifySecurityStamp(ClaimsPrincipal principal)
            => SignInManager.ValidateSecurityStampAsync(principal);

        public virtual async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            DateTimeOffset currentUTC = DateTimeOffset.UtcNow;
            if (context.Options != null && Clock != null)
                currentUTC = Clock.UtcNow;

            var issuedUtc = context.Properties.IssuedUtc;
            var validate = (issuedUtc == null);
            if(issuedUtc != null)
            {
                var timeElapsed = currentUTC.Subtract(issuedUtc.Value);
                validate = timeElapsed > Options.ValidationInterval;
            }
            if(validate)
            {
                var user = await VerifySecurityStamp(context.Principal);
                if (user != null)
                    await SecurityStampVerified(user, context);
                else
                {
                    Logger.LogDebug(LoggerEventIds.SecurityStampValidationFailed, "Security stamp validation failed! Rejecting cookie.");
                    context.RejectPrincipal();
                    await SignInManager.SignOutAsync();
                }
            }
        }
    }

    public static class SecurityStampValidator
    {
        public static Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
            => ValidateAsync<ISecurityStampValidator>(context);

        public static Task ValidateAsync<TValidator>(CookieValidatePrincipalContext context) where TValidator : ISecurityStampValidator
        {
            if (context.HttpContext.RequestServices == null)
                throw new InvalidOperationException("RequestServices is null!");

            TValidator validator = context.HttpContext.RequestServices.GetRequiredService<TValidator>();
            return validator.ValidateAsync(context);
        }
    }
}