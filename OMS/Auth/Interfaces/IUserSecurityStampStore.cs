using System.Threading;
using System.Threading.Tasks;

namespace OMS.Auth
{
    public interface IUserSecurityStampStore<TUser> : IUserStore<TUser> where TUser : class
    {
        Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken);
        Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken);
    }
}