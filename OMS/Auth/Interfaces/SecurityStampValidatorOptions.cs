using System;
using System.Threading.Tasks;
using System.Security.Claims;

namespace OMS.Auth
{
    public class SecurityStampValidatorOptions
    {
        public TimeSpan ValidationInterval { get; set; } = TimeSpan.FromMinutes(30);

        public Func<SecurityStampRefreshingPrincipalContext, Task> OnRefreshingPrincipal { get; set; }
    }

    public class SecurityStampRefreshingPrincipalContext
    {
        public ClaimsPrincipal CurrentPrincipal { get; set; }
        public ClaimsPrincipal NewPrincipal { get; set; }
    }
}