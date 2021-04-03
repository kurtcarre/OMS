using System.Threading;
using System.Threading.Tasks;

namespace OMS.Auth
{
    public interface IPasswordValidator<TUser> where TUser : class
    {
        Task<AuthResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password);
    }
}