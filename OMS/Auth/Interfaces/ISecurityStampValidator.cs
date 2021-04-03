using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace OMS.Auth
{
    public interface ISecurityStampValidator
    {
        Task ValidateAsync(CookieValidatePrincipalContext context);
    }
}