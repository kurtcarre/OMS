namespace OMS.Auth
{
    public interface IProtectedUserStore<TUser> : IUserStore<TUser> where TUser : class
    {

    }
}