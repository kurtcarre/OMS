using System.Threading;
using System.Threading.Tasks;

namespace OMS.Auth
{
    public interface IUserEmailStore<TUser> : IUserStore<TUser> where TUser : class
    {
        Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken);
        Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken);
        Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken);
        Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken);
        Task<TUser> FindByEmailAsync(string email, CancellationToken cancellationToken);
    }
}