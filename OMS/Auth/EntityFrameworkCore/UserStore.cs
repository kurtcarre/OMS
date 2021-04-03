using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OMS.Auth.EntityFrameworkCore
{
    public class UserStore : UserStore<User<string>>
    {
        public UserStore(DbContext context, AuthErrorDescriber describer = null) : base(context, describer)
        {

        }
    }
    // ADD ROLE HERE
    public class UserStore<TUser> : UserStore<TUser, DbContext, string>
        where TUser : User<string>, new()
    {
        public UserStore(DbContext context, AuthErrorDescriber describer = null) : base(context, describer)
        {

        }
    }
    // ADD ROLE HERE
    public class UserStore<TUser, TContext> : UserStore<TUser, TContext, string>
        where TUser : User<string>
        where TContext : DbContext
    {
        public UserStore(TContext context, AuthErrorDescriber describer = null) : base(context, describer)
        {

        }
    }
    // ADD ROLE HERE
    public class UserStore<TUser, TContext, TKey> : UserStore<TUser, TContext, TKey, UserClaim<TKey>, UserLogin<TKey>>
        where TUser : User<TKey>
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public UserStore(TContext context, AuthErrorDescriber describer = null) : base(context, describer)
        {

        }
    }
    // ADD ROLE HERE
    public class UserStore<TUser, TContext, TKey, TUserClaim, TUserLogin> : UserStoreBase<TUser, TKey, TUserClaim, TUserLogin>, IProtectedUserStore<TUser>
        where TUser : User<TKey>
        where TContext : DbContext
        where TKey : IEquatable<TKey>
        where TUserClaim : UserClaim<TKey>, new()
        where TUserLogin : UserLogin<TKey>, new()
    {
        public UserStore(TContext context, AuthErrorDescriber describer = null) : base(describer ?? new AuthErrorDescriber())
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;
        }

        public virtual TContext Context { get; private set; }

        private DbSet<TUser> UsersSet { get { return Context.Set<TUser>(); } }
        private DbSet<TUserClaim> UserClaims { get { return Context.Set<TUserClaim>(); } }
        private DbSet<TUserLogin> UserLogins { get { return Context.Set<TUserLogin>(); } }

        public bool AutoSaveChanges { get; set; } = true;

        protected Task SaveChanges(CancellationToken cancellationToken)
        {
            return AutoSaveChanges ? Context.SaveChangesAsync(cancellationToken) : Task.CompletedTask;
        }

        public override async Task<AuthResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            Context.Add(user);
            await SaveChanges(cancellationToken);
            return AuthResult.Success;
        }

        public async override Task<AuthResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            Context.Attach(user);
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            Context.Update(user);
            await SaveChanges(cancellationToken);
            return AuthResult.Success;
        }

        public async override Task<AuthResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            Context.Remove(user);
            try
            {
                await SaveChanges(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return AuthResult.Failed(ErrorDescriber.ConcurrencyFailure());
            }
            return AuthResult.Success;
        }

        public override Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            TKey id = ConvertIdFromString(userId);
            return UsersSet.FindAsync(new object[] { id }, cancellationToken).AsTask();
        }

        public override Task<TUser> FindByNameAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Users.FirstOrDefaultAsync(u => u.UserName == username, cancellationToken);
        }

        public override IQueryable<TUser> Users
        {
            get { return UsersSet; }
        }

        protected override Task<TUser> FindUserAsync(TKey userId, CancellationToken cancellationToken)
        {
            return Users.SingleOrDefaultAsync(u => u.Id.Equals(userId), cancellationToken);
        }

        protected override Task<TUserLogin> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return UserLogins.SingleOrDefaultAsync(userLogin => userLogin.UserId.Equals(userId), cancellationToken);
        }

        protected override Task<TUserLogin> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return UserLogins.SingleOrDefaultAsync(userLogin => userLogin.LoginProvider == loginProvider && userLogin.ProviderKey == providerKey, cancellationToken);
        }

        public async override Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return await UserClaims.Where(uc => uc.UserId.Equals(user.Id)).Select(c => c.ToClaim()).ToListAsync(cancellationToken);
        }

        public override Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }
            foreach (var claim in claims)
            {
                UserClaims.Add(CreateUserClaim(user, claim));
            }
            return Task.FromResult(false);
        }

        public async override Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }
            if (newClaim == null)
            {
                throw new ArgumentNullException(nameof(newClaim));
            }

            var matchedClaims = await UserClaims.Where(uc => uc.UserId.Equals(user.Id) && uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToListAsync(cancellationToken);
            foreach (var matchedClaim in matchedClaims)
            {
                matchedClaim.ClaimValue = newClaim.Value;
                matchedClaim.ClaimType = newClaim.Type;
            }
        }

        public async override Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }
            foreach (var claim in claims)
            {
                var matchedClaims = await UserClaims.Where(uc => uc.UserId.Equals(user.Id) && uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToListAsync(cancellationToken);
                foreach (var c in matchedClaims)
                {
                    UserClaims.Remove(c);
                }
            }
        }

        public override Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }
            UserLogins.Add(CreateUserLogin(user, login));
            return Task.FromResult(false);
        }

        public override async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            TUserLogin entry = await FindUserLoginAsync(user.Id, loginProvider, providerKey, cancellationToken);
            if (entry != null)
                UserLogins.Remove(entry);
        }

        public async override Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            TKey userId = user.Id;
            return await UserLogins.Where(l => l.UserId.Equals(userId)).Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName)).ToListAsync(cancellationToken);
        }

        public async override Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            TUserLogin userLogin = await FindUserLoginAsync(loginProvider, providerKey, cancellationToken);
            if (userLogin != null)
                return await FindUserAsync(userLogin.UserId, cancellationToken);

            return null;
        }

        public override Task<TUser> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async override Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            var query = from userclaims in UserClaims
                        join user in Users on userclaims.UserId equals user.Id
                        where userclaims.ClaimValue == claim.Value
                        && userclaims.ClaimType == claim.Type
                        select user;

            return await query.ToListAsync(cancellationToken);
        }
    }
}