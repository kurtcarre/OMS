using System.Threading.Tasks;

namespace OMS.Auth
{
    public class DefaultUserConfirmation<TUser> : IUserConfirmation<TUser> where TUser : class
    {
        public async virtual Task<bool> IsConfirmedAsync(UserManager<TUser> manager, TUser user)
        {
            return await manager.IsEmailConfirmedAsync(user);
        }
    }
}