using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OMS.Auth.Models;
using OMS.Data;

namespace OMS.Auth.Services
{
    public class UserManager
    {
        private readonly DBContext _context;
        private readonly PasswordHasher hasher;

        public UserManager(DBContext context, PasswordHasher _hasher)
        {
            _context = context;
            hasher = _hasher;
        }

        private DbSet<User> UserSet
        {
            get
            {
                return _context.Set<User>();
            }
        }

        public IQueryable<User> Users
        {
            get
            {
                return UserSet;
            }
        }

        public async Task CreateUser(User newUser)
        {
            await _context.AddAsync(newUser);
            await _context.SaveChangesAsync();
        }

        public async Task CreateUser(User newUser, string password)
        {
            string hash = hasher.HashPassword(password);
            newUser.PasswordHash = hash;
            await _context.AddAsync(newUser);
            await _context.SaveChangesAsync();
        }

        public async Task AmendUser(User user)
        {
            _context.Attach(user);
            _context.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(User user)
        {
            _context.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> FindUserById(string id)
        {
            User user = await UserSet.FindAsync(id);
            return user;
        }

        public async Task<User> FindUserByUsername(string username)
        {
            User user = await Users.FirstOrDefaultAsync(u => u.UserName == username);
            return user;
        }

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            string userPassword = user.PasswordHash;
            var result = await hasher.VerifyHashedPassword(userPassword, password);
            if (result == SignInResult.Success)
                return true;

            else if(result == SignInResult.RehashNeeded)
            {
                _context.Attach(user);
                user.PasswordHash = hasher.HashPassword(password);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task AdminResetPasswordAsync(User user, string newPassword)
        {
            string hash = hasher.HashPassword(newPassword);
            user.PasswordHash = hash;
            await AmendUser(user);
        }
    }
}