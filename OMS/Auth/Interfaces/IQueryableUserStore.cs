using System.Linq;

namespace OMS.Auth
{
    public interface IQueryableUserStore<TUser> : IUserStore<TUser> where TUser : class
    {
        IQueryable<TUser> Users { get; }
    }
}