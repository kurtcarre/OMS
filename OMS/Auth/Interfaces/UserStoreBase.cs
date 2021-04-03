using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Auth
{
    public abstract class UserStoreBase<TUser, TKey, TUserClaim, TUserLogin> :
        IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IUserEmailStore<TUser>,
        IQueryableUserStore<TUser>
        where TUser : User<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : UserClaim<TKey>, new()
        where TUserLogin : UserLogin<TKey>, new()
    {
        public UserStoreBase(AuthErrorDescriber describer)
        {
            if (describer == null)
                throw new ArgumentNullException(nameof(describer));

            ErrorDescriber = describer;
        }

        private bool _disposed;

        public AuthErrorDescriber ErrorDescriber { get; set; }

        protected virtual TUserClaim CreateUserClaim(TUser user, Claim claim)
        {
            TUserClaim userClaim = new TUserClaim { UserId = user.Id };
            userClaim.InitialiseFromClaim(claim);
            return userClaim;
        }

        protected virtual TUserLogin CreateUserLogin(TUser user, UserLoginInfo login)
        {
            return new TUserLogin
            {
                UserId = user.Id,
                ProviderKey = login.ProviderKey,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName
            };
        }

        public virtual Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(ConvertIdToString(user.Id));
        }

        public virtual Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.UserName);
        }

        public virtual Task SetUserNameAsync(TUser user, string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.UserName = username;
            return Task.CompletedTask;
        }

        public abstract Task<AuthResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken));
        public abstract Task<AuthResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken));
        public abstract Task<AuthResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken));
        public abstract Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken));

        public virtual TKey ConvertIdFromString(string id)
        {
            if (id == null)
                return default(TKey);

            return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id);
        }

        public virtual string ConvertIdToString(TKey id)
        {
            if (object.Equals(id, default(TKey)))
                return null;

            return id.ToString();
        }

        public abstract Task<TUser> FindByNameAsync(string username, CancellationToken cancellationToken = default(CancellationToken));

        public abstract IQueryable<TUser> Users { get; }

        public virtual Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public virtual Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PasswordHash);
        }

        protected abstract Task<TUser> FindUserAsync(TKey userId, CancellationToken cancellationToken);

        protected abstract Task<TUserLogin> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken);

        protected abstract Task<TUserLogin> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken);

        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public void Dispose()
        {
            _disposed = true;
        }

        public abstract Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken));

        public async virtual Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            TUserLogin userLogin = await FindUserLoginAsync(loginProvider, providerKey, cancellationToken);
            if (userLogin != null)
                return await FindUserAsync(userLogin.UserId, cancellationToken);

            return null;
        }

        public virtual Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.EmailConfirmed);
        }

        public virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public virtual Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.Email = email;
            return Task.CompletedTask;
        }

        public virtual Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.Email);
        }

        public abstract Task<TUser> FindByEmailAsync(string email, CancellationToken cancellationToken = default(CancellationToken));

        public virtual Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (stamp == null)
                throw new ArgumentNullException(nameof(stamp));

            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public virtual Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.SecurityStamp);
        }

        public abstract Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default(CancellationToken));
    }
}