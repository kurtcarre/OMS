using System;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Auth
{
    public interface IUserStore<TUser> : IDisposable where TUser : class
    {
        Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken);
        Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken);

        Task SetUserNameAsync(TUser user, string username, CancellationToken cancellationToken);

        Task<AuthResult> CreateAsync(TUser user, CancellationToken cancellationToken);
        Task<AuthResult> UpdateAsync(TUser user, CancellationToken cancellationToken);
        Task<AuthResult> DeleteAsync(TUser user, CancellationToken cancellationToken);

        Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken);
        Task<TUser> FindByNameAsync(string username, CancellationToken cancellationToken);
    }
}