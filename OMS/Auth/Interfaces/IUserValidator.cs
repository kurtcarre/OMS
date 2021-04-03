using System.Threading;
using System.Threading.Tasks;

namespace OMS.Auth
{
    public interface IUserValidator<TUser> where TUser : class
    {
        Task<AuthResult> ValidateAsync(UserManager<TUser> manager, TUser user);
    }
}