using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Http;
using OMS.Auth.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthServiceCollectionExtensions
    {
        public static void AddAuth(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "OMS.Auth";
                options.DefaultChallengeScheme = "OMS.Auth";
                options.DefaultSignInScheme = "OMS.Auth";
            }).AddCookie("OMS.Auth", o =>
            {
                o.LoginPath = new PathString("/Auth/Account/Login");
                o.AccessDeniedPath = new PathString("/Auth/AccessDenied");
            });

            services.AddHttpContextAccessor();
            services.TryAddScoped<PasswordHasher>();
            services.TryAddScoped<UserManager>();
            services.TryAddScoped<RoleManager>();
            services.TryAddScoped<SignInManager>();
        }
    }
}