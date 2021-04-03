using System.Threading.Tasks;

namespace OMS.Auth
{
    public interface IUserConfirmation<TUser> where TUser : class
    {
        Task<bool> IsConfirmedAsync(UserManager<TUser> manager, TUser user);
    }
}