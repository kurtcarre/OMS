using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OMS.Auth
{
    public class UserManager<TUser> : IDisposable where TUser : class
    {
        public const string ResetPasswordTokenPurpose = "ResetPassword";
        public const string ConfirmEmailTokenPurpose = "ConfirmEmail";

        private bool _disposed;
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        private IServiceProvider _services;

        protected virtual CancellationToken CancellationToken => CancellationToken.None;

        public UserManager(IUserStore<TUser> store, IOptions<AuthOptions> options, IPasswordHasher<TUser> passwordHasher, IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators, AuthErrorDescriber errors, IServiceProvider services, ILogger<UserManager<TUser>> logger)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));
            
            Store = store;
            Options = options?.Value ?? new AuthOptions();
            PasswordHasher = passwordHasher;
            ErrorDescriber = errors;
            Logger = logger;

            if(userValidators != null)
            {
                foreach(var v in userValidators)
                {
                    UserValidators.Add(v);
                }
            }
            if (passwordValidators != null)
            {
                foreach (var v in passwordValidators)
                {
                    PasswordValidators.Add(v);
                }
            }

            _services = services;
            
            if(Options.Stores.ProtectPersonalData)
            {
                if (!(Store is IProtectedUserStore<TUser>))
                    throw new InvalidOperationException("Store is not a protected user store!");
                if (services.GetService<ILookupProtector>() == null)
                    throw new InvalidOperationException("There's no personal data protector!");
            }
        }

        protected internal IUserStore<TUser> Store { get; set; }

        public virtual ILogger Logger { get; set; }
        public IPasswordHasher<TUser> PasswordHasher { get; set; }
        public IList<IUserValidator<TUser>> UserValidators { get; } = new List<IUserValidator<TUser>>();
        public IList<IPasswordValidator<TUser>> PasswordValidators { get; } = new List<IPasswordValidator<TUser>>();
        public AuthErrorDescriber ErrorDescriber { get; set; }
        public AuthOptions Options { get; set; }

        public virtual bool SupportsUserSecurityStamp
        {
            get
            {
                ThrowIfDisposed();
                return Store is IUserSecurityStampStore<TUser>;
            }
        }

        public virtual bool SupportsUserRole
        {
            get
            {
                ThrowIfDisposed();
                return false;
            }
        }

        public virtual bool SupportsUserEmail
        {
            get
            {
                ThrowIfDisposed();
                return Store is IUserEmailStore<TUser>;
            }
        }

        public virtual bool SupportsUserClaim
        {
            get
            {
                ThrowIfDisposed();
                return Store is IUserClaimStore<TUser>;
            }
        }

        public virtual bool SupportsQueryableUsers
        {
            get
            {
                ThrowIfDisposed();
                return Store is IQueryableUserStore<TUser>;
            }
        }

        public virtual IQueryable<TUser> Users
        {
            get
            {
                var queryableStore = Store as IQueryableUserStore<TUser>;
                if (queryableStore == null)
                    throw new NotSupportedException("Queryable stores are not supported!");
                return queryableStore.Users;
            }
        }

        public virtual string GetUserName(ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirstValue(Options.Claims.UserNameClaimType);
        }

        public virtual string GetUserId(ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirstValue(Options.Claims.UserIdClaimType);
        }

        public virtual Task<TUser> GetUserAsync(ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            string id = GetUserId(principal);
            return id == null ? Task.FromResult<TUser>(null) : FindByIdAsync(id);
        }

        public virtual Task<string> GenerateConcurrencyStampAsync(TUser user)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public virtual async Task<AuthResult> CreateAsync(TUser user)
        {
            ThrowIfDisposed();
            await UpdateSecurityStampInternal(user);
            var result = await ValidateUserAsync(user);
            if (!result.Succeeded)
                return result;
            return await Store.CreateAsync(user, CancellationToken);
        }

        public virtual Task<AuthResult> UpdateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return UpdateUserAsync(user);
        }

        public virtual Task<AuthResult> DeleteAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Store.DeleteAsync(user, CancellationToken);
        }

        public virtual Task<TUser> FindByIdAsync(string userId)
        {
            ThrowIfDisposed();
            return Store.FindByIdAsync(userId, CancellationToken);
        }

        public virtual async Task<TUser> FindByNameAsync(string username)
        {
            ThrowIfDisposed();
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            TUser user = await Store.FindByNameAsync(username, CancellationToken);

            if(user == null && Options.Stores.ProtectPersonalData)
            {
                ILookupProtectorKeyRing keyRing = _services.GetService<ILookupProtectorKeyRing>();
                ILookupProtector protector = _services.GetService<ILookupProtector>();
                if(keyRing != null && protector != null)
                {
                    foreach(var key in keyRing.GetAllKeyIds())
                    {
                        var oldKey = protector.Protect(key, username);
                        user = await Store.FindByNameAsync(oldKey, CancellationToken);
                        if (user != null)
                            return user;
                    }
                }
            }
            return user;
        }

        public virtual async Task<AuthResult> CreateAsync(TUser user, string password)
        {
            ThrowIfDisposed();
            IUserPasswordStore<TUser> passwordStore = GetPasswordStore();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            AuthResult result = await UpdatePasswordHash(passwordStore, user, password);
            if (!result.Succeeded)
                return result;

            return await CreateAsync(user);
        }

        private string ProtectPersonalData(string data)
        {
            if(Options.Stores.ProtectPersonalData)
            {
                ILookupProtectorKeyRing keyRing = _services.GetService<ILookupProtectorKeyRing>();
                ILookupProtector protector = _services.GetService<ILookupProtector>();
                return protector.Protect(keyRing.CurrentKeyId, data);
            }
            return data;
        }

        public virtual async Task<string> GetUserNameAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return await Store.GetUserNameAsync(user, CancellationToken);
        }

        public virtual async Task<AuthResult> SetUserNameAsync(TUser user, string username)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await Store.SetUserNameAsync(user, username, CancellationToken);
            await UpdateSecurityStampInternal(user);
            return await UpdateUserAsync(user);
        }

        public virtual async Task<string> GetUserIdAsync(TUser user)
        {
            ThrowIfDisposed();
            return await Store.GetUserIdAsync(user, CancellationToken);
        }

        public virtual async Task<bool> CheckPasswordAsync(TUser user, string password)
        {
            ThrowIfDisposed();
            IUserPasswordStore<TUser> passwordStore = GetPasswordStore();
            if (user == null)
                return false;

            PasswordVerificationResult result = await VerifyPasswordAsync(passwordStore, user, password);
            if(result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                await UpdatePasswordHash(passwordStore, user, password, validatePassword: false);
                await UpdateUserAsync(user);
            }

            bool success = result != PasswordVerificationResult.Failed;
            if (!success)
                Logger.LogWarning(LoggerEventIds.InvalidPassword, "Invalid password!");

            return success;
        }

        public virtual async Task<AuthResult> ChangePasswordAsync(TUser user, string currentPassword, string newPassword)
        {
            ThrowIfDisposed();
            IUserPasswordStore<TUser> passwordStore = GetPasswordStore();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if(await VerifyPasswordAsync(passwordStore, user, currentPassword) != PasswordVerificationResult.Failed)
            {
                AuthResult result = await UpdatePasswordHash(passwordStore, user, newPassword);
                if (!result.Succeeded)
                    return result;

                return await UpdateUserAsync(user);
            }

            Logger.LogWarning(LoggerEventIds.ChangePasswordFailed, "Password change failed!");
            return AuthResult.Failed(ErrorDescriber.PasswordMismath());
        }

        protected virtual async Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<TUser> store, TUser user, string password)
        {
            string hash = await store.GetPasswordHashAsync(user, CancellationToken);
            if (hash == null)
                return PasswordVerificationResult.Failed;

            return PasswordHasher.VerifyHashedPassword(user, hash, password);
        }

        public virtual async Task<string> GetSecurityStampAsync(TUser user)
        {
            ThrowIfDisposed();
            IUserSecurityStampStore<TUser> securityStampStore = GetSecurityStampStore();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            string stamp = await securityStampStore.GetSecurityStampAsync(user, CancellationToken);
            if(stamp == null)
            {
                Logger.LogWarning(LoggerEventIds.GetSecurityStampFailed, "User stamp null!");
                throw new InvalidOperationException("Null security stamp!");
            }
            return stamp;
        }

        public virtual async Task<AuthResult> UpdateSecurityStampAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await UpdateSecurityStampInternal(user);
            return await UpdateUserAsync(user);
        }

        public virtual Task<string> GeneratePasswordResetTokenAsync(TUser user)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<AuthResult> ResetPasswordAsync(TUser user, string token, string newPassword)
        {
            ThrowIfDisposed();
            IUserPasswordStore<TUser> store = GetPasswordStore();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

//            if (false)
//                return AuthResult.Failed(ErrorDescriber.InvalidToken());

            var result = await UpdatePasswordHash(store, user, newPassword, validatePassword: true);
            if (!result.Succeeded)
                return result;

            return await UpdateUserAsync(user);
        }

        public virtual async Task<AuthResult> AdminResetPasswordAsync(TUser user, string newPassword)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            IUserPasswordStore<TUser> store = GetPasswordStore();

            var result = await UpdatePasswordHash(store, user, newPassword, validatePassword: true);
            if (!result.Succeeded)
                return result;

            return await UpdateUserAsync(user);
        }

        public virtual Task<TUser> FindByLoginAsync(string loginProvider, string providerKey)
        {
            ThrowIfDisposed();
            var loginStore = GetLoginStore();
            if (loginProvider == null)
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }
            if (providerKey == null)
            {
                throw new ArgumentNullException(nameof(providerKey));
            }
            return loginStore.FindByLoginAsync(loginProvider, providerKey, CancellationToken);
        }

        public virtual async Task<AuthResult> RemoveLoginAsync(TUser user, string loginProvider, string providerKey)
        {
            ThrowIfDisposed();
            var loginStore = GetLoginStore();
            if (loginProvider == null)
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }
            if (providerKey == null)
            {
                throw new ArgumentNullException(nameof(providerKey));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await loginStore.RemoveLoginAsync(user, loginProvider, providerKey, CancellationToken);
            await UpdateSecurityStampInternal(user);
            return await UpdateUserAsync(user);
        }

        public virtual async Task<AuthResult> AddLoginAsync(TUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            var loginStore = GetLoginStore();
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var existingUser = await FindByLoginAsync(login.LoginProvider, login.ProviderKey);
            if (existingUser != null)
            {
                Logger.LogWarning(LoggerEventIds.AddLoginFailed, "AddLogin for user failed because it was already associated with another user.");
                return AuthResult.Failed(ErrorDescriber.LoginAlreadyAssociated());
            }
            await loginStore.AddLoginAsync(user, login, CancellationToken);
            return await UpdateUserAsync(user);
        }

        public virtual async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            ThrowIfDisposed();
            var loginStore = GetLoginStore();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return await loginStore.GetLoginsAsync(user, CancellationToken);
        }

        public virtual Task<AuthResult> AddClaimAsync(TUser user, Claim claim)
        {
            ThrowIfDisposed();
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));
            
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return AddClaimsAsync(user, new Claim[] { claim });
        }

        public virtual async Task<AuthResult> AddClaimsAsync(TUser user, IEnumerable<Claim> claims)
        {
            ThrowIfDisposed();
            IUserClaimStore<TUser> claimStore = GetClaimStore();
            if (claims == null)
                throw new ArgumentNullException(nameof(claims));

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await claimStore.AddClaimsAsync(user, claims, CancellationToken);
            return await UpdateUserAsync(user);
        }

        public virtual async Task<AuthResult> ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim)
        {
            ThrowIfDisposed();
            var claimStore = GetClaimStore();
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }
            if (newClaim == null)
            {
                throw new ArgumentNullException(nameof(newClaim));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await claimStore.ReplaceClaimAsync(user, claim, newClaim, CancellationToken);
            return await UpdateUserAsync(user);
        }

        public virtual Task<AuthResult> RemoveClaimAsync(TUser user, Claim claim)
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
            return RemoveClaimsAsync(user, new Claim[] { claim });
        }

        public virtual async Task<AuthResult> RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims)
        {
            ThrowIfDisposed();
            var claimStore = GetClaimStore();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            await claimStore.RemoveClaimsAsync(user, claims, CancellationToken);
            return await UpdateUserAsync(user);
        }

        public virtual async Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            ThrowIfDisposed();
            var claimStore = GetClaimStore();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return await claimStore.GetClaimsAsync(user, CancellationToken);
        }

        public virtual async Task<string> GetEmailAsync(TUser user)
        {
            ThrowIfDisposed();
            IUserEmailStore<TUser> emailStore = GetEmailStore();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return await emailStore.GetEmailAsync(user, CancellationToken);
        }

        public virtual async Task<AuthResult> SetEmailAsync(TUser user, string email)
        {
            ThrowIfDisposed();
            IUserEmailStore<TUser> emailStore = GetEmailStore();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await emailStore.SetEmailAsync(user, email, CancellationToken);
            await emailStore.SetEmailConfirmedAsync(user, false, CancellationToken);
            await UpdateSecurityStampInternal(user);
            return await UpdateUserAsync(user);
        }

        public virtual async Task<TUser> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();
            IUserEmailStore<TUser> emailStore = GetEmailStore();
            if (email == null)
                throw new ArgumentNullException(nameof(email));

            TUser user = await emailStore.FindByEmailAsync(email, CancellationToken);
            if(user == null && Options.Stores.ProtectPersonalData)
            {
                ILookupProtectorKeyRing keyRing = _services.GetService<ILookupProtectorKeyRing>();
                ILookupProtector protector = _services.GetService<ILookupProtector>();
                if(keyRing != null && protector != null)
                {
                    foreach(var key in keyRing.GetAllKeyIds())
                    {
                        string oldKey = protector.Protect(key, email);
                        user = await emailStore.FindByEmailAsync(oldKey, CancellationToken);
                        if (user != null)
                            return user;
                    }
                }
            }
            return user;
        }

        public virtual Task<string> GenerateEmailConfirmationTokenAsync(TUser user)
        {
            ThrowIfDisposed();
            throw new NotImplementedException();
        }

        public virtual async Task<AuthResult> ConfirmEmailAsync(TUser user, string token)
        {
            ThrowIfDisposed();
            IUserEmailStore<TUser> emailStore = GetEmailStore();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

 //           if(false)
 //               return AuthResult.Failed(ErrorDescriber.InvalidToken());

            await emailStore.SetEmailConfirmedAsync(user, true, CancellationToken);
            return await UpdateUserAsync(user);
        }

        public virtual async Task<bool> IsEmailConfirmedAsync(TUser user)
        {
            ThrowIfDisposed();
            IUserEmailStore<TUser> emailStore = GetEmailStore();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return await emailStore.GetEmailConfirmedAsync(user, CancellationToken);
        }

        public virtual async Task<AuthResult> ChangeEmailAsync(TUser user, string newEmail, string token)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

//            if (false)
//                return AuthResult.Failed(ErrorDescriber.InvalidToken());

            IUserEmailStore<TUser> emailStore = GetEmailStore();
            await emailStore.SetEmailAsync(user, newEmail, CancellationToken);
            await emailStore.SetEmailConfirmedAsync(user, true, CancellationToken);
            await UpdateSecurityStampInternal(user);
            return await UpdateUserAsync(user);
        }

        public virtual async Task<bool> VerifyUserTokenAsync(TUser user, string tokenProvider, string purpose, string token)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (tokenProvider == null)
                throw new ArgumentNullException(nameof(tokenProvider));

            await Task.Delay(1);

            throw new NotImplementedException();
        }

        public virtual Task<string> GenerateUserTokenAsync(TUser user, string tokenProvider, string purpose)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (tokenProvider == null)
            {
                throw new ArgumentNullException(nameof(tokenProvider));
            }

            throw new NotImplementedException();
        }

        public virtual Task<IList<TUser>> GetUsersForClaimAsync(Claim claim)
        {
            ThrowIfDisposed();
            IUserClaimStore<TUser> claimStore = GetClaimStore();
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            return claimStore.GetUsersForClaimAsync(claim, CancellationToken);
        }

        private IUserEmailStore<TUser> GetEmailStore(bool throwOnFail = true)
        {
            IUserEmailStore<TUser> store = Store as IUserEmailStore<TUser>;
            if (throwOnFail && store == null)
                throw new NotSupportedException("Store isn't an email store!");

            return store;
        }

        private async Task UpdateSecurityStampInternal(TUser user)
        {
            if (SupportsUserSecurityStamp)
                await GetSecurityStampStore().SetSecurityStampAsync(user, NewSecurityStamp(), CancellationToken);
        }

        private async Task<AuthResult> UpdatePasswordHash(IUserPasswordStore<TUser> passwordStore, TUser user, string newPassword, bool validatePassword = true)
        {
            if(validatePassword)
            {
                AuthResult validate = await ValidatePasswordAsync(user, newPassword);
                if (!validate.Succeeded)
                    return validate;
            }

            string hash = newPassword != null ? PasswordHasher.HashPassword(user, newPassword) : null;
            await passwordStore.SetPasswordHashAsync(user, hash, CancellationToken);
            await UpdateSecurityStampInternal(user);
            return AuthResult.Success;
        }

        private static string NewSecurityStamp()
        {
            byte[] bytes = new byte[20];

            _rng.GetBytes(bytes);

            return Base32.ToBase32(bytes);
        }

        private IUserLoginStore<TUser> GetLoginStore()
        {
            IUserLoginStore<TUser> store = Store as IUserLoginStore<TUser>;
            if (store == null)
                throw new NotSupportedException("Store isn't a login store!");

            return store;
        }

        private IUserSecurityStampStore<TUser> GetSecurityStampStore()
        {
            IUserSecurityStampStore<TUser> store = Store as IUserSecurityStampStore<TUser>;
            if (store == null)
                throw new NotSupportedException("Store isn't a security stamp store!");

            return store;
        }

        private IUserClaimStore<TUser> GetClaimStore()
        {
            var cast = Store as IUserClaimStore<TUser>;
            if (cast == null)
            {
                throw new NotSupportedException("Store isn't a claim store!");
            }
            return cast;
        }

        public static string GetChangeEmailTokenPurpose(string newEmail) => "ChangeEmail:" + newEmail;

        protected async Task<AuthResult> ValidateUserAsync(TUser user)
        {
            if(SupportsUserSecurityStamp)
            {
                string stamp = await GetSecurityStampAsync(user);
                if (stamp == null)
                    throw new InvalidOperationException("Security stamp is null!");
            }

            List<AuthError> errors = new List<AuthError>();
            foreach(var v in UserValidators)
            {
                var result = await v.ValidateAsync(this, user);
                if (!result.Succeeded)
                    errors.AddRange(result.Errors);
            }
            if(errors.Count > 0)
            {
                Logger.LogWarning(LoggerEventIds.UserValidationFailure, "User validation error: {errors}.", string.Join(";", errors.Select(e => e.Code)));
                return AuthResult.Failed(errors.ToArray());
            }
            return AuthResult.Success;
        }

        protected async Task<AuthResult> ValidatePasswordAsync(TUser user, string password)
        {
            List<AuthError> errors = new List<AuthError>();
            bool isValid = true;
            foreach(var v in PasswordValidators)
            {
                AuthResult result = await v.ValidateAsync(this, user, password);
                if(!result.Succeeded)
                {
                    if (result.Errors.Any())
                        errors.AddRange(result.Errors);

                    isValid = false;
                }
            }
            if(!isValid)
            {
                Logger.LogWarning(LoggerEventIds.PasswordValidationFailure, "User password validation failed: {errors}.", string.Join(";", errors.Select(e => e.Code)));
                return AuthResult.Failed(errors.ToArray());
            }
            return AuthResult.Success;
        }

        protected virtual async Task<AuthResult> UpdateUserAsync(TUser user)
        {
            AuthResult result = await ValidateUserAsync(user);
            if (!result.Succeeded)
                return result;

            return await Store.UpdateAsync(user, CancellationToken);
        }

        private IUserPasswordStore<TUser> GetPasswordStore()
        {
            IUserPasswordStore<TUser> store = Store as IUserPasswordStore<TUser>;
            if (store == null)
                throw new NotSupportedException("Store isn't a password store!");

            return store;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing && !_disposed)
            {
                Store.Dispose();
                _disposed = true;
            }
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}