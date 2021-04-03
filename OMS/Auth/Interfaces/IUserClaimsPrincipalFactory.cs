using System.Security.Claims;
using System.Threading.Tasks;

namespace OMS.Auth
{
    public interface IUserClaimsPrincipalFactory<TUser> where TUser : class
    {
        Task<ClaimsPrincipal> CreateAsync(TUser user);
    }
}