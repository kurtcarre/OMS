using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Auth
{
    public interface IUserLoginStore<TUser> : IUserStore<TUser> where TUser : class
    {
        Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken);
        Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken);
        Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken);
        Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken);
    }
}