using System.Threading;
using System.Threading.Tasks;

namespace OMS.Auth
{
    public interface IUserPasswordStore<TUser> : IUserStore<TUser> where TUser : class
    {
        Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken);
        Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken);
    }
}