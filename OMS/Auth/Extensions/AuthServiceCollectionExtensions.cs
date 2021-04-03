using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OMS.Auth;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthServiceCollectionExtensions
    {
        public static AuthBuilder AddAuthCore<TUser>(this IServiceCollection services) where TUser : class
            => services.AddAuthCore<TUser>(o => { });

        public static AuthBuilder AddAuthCore<TUser>(this IServiceCollection services, Action<AuthOptions> setupAction) where TUser : class
        {
            services.AddOptions().AddLogging();

            services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
            services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
            services.TryAddScoped<IUserConfirmation<TUser>, DefaultUserConfirmation<TUser>>();
            services.TryAddScoped<AuthErrorDescriber>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser>>();
            services.TryAddScoped<UserManager<TUser>>();

            if (setupAction != null)
                services.Configure(setupAction);

            return new AuthBuilder(typeof(TUser), services);
        }

        public static AuthBuilder AddIdentity<TUser>(this IServiceCollection services) where TUser : class
            => services.AddIdentity<TUser>(setupAction: null);

        public static AuthBuilder AddIdentity<TUser>(this IServiceCollection services, Action<AuthOptions> setupAction) where TUser : class
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthConstants.AuthSchema;
                options.DefaultChallengeScheme = AuthConstants.AuthSchema;
                options.DefaultSignInScheme = AuthConstants.AuthSchema;
            }).AddCookie(AuthConstants.AuthSchema, o =>
            {
                o.LoginPath = new PathString("/Auth/Account/Login");
                o.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
                };
            });

            services.AddHttpContextAccessor();
            services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
            services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
            services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
            services.TryAddScoped<AuthErrorDescriber>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser>>();
            services.TryAddScoped<IUserConfirmation<TUser>, DefaultUserConfirmation<TUser>>();
            services.TryAddScoped<UserManager<TUser>>();
            services.TryAddScoped<SignInManager<TUser>>();

            if (setupAction != null)
                services.Configure(setupAction);

            return new AuthBuilder(typeof(TUser), services);
        }

        public static IServiceCollection ConfigureApplicationCookie(this IServiceCollection services, Action<CookieAuthenticationOptions> configure)
        {
            services.Configure(AuthConstants.ApplicationSchema, configure);
            return services.Configure(AuthConstants.AuthSchema, configure);
        }
    }
}