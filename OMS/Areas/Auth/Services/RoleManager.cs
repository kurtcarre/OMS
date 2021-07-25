using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OMS.Auth.Models;
using OMS.Data;

namespace OMS.Auth.Services
{
    public class RoleManager
    {
        private readonly DBContext _context;

        public RoleManager(DBContext context)
        {
            _context = context;
        }

        private DbSet<Role> RoleSet
        {
            get
            {
                return _context.Set<Role>();
            }
        }

        public IQueryable<Role> Roles
        {
            get
            {
                return RoleSet;
            }
        }

        public async Task CreateAsync(Role role)
        {
            await _context.AddAsync(role);
            await _context.SaveChangesAsync();
        }

        public async Task AmendAsync(Role role)
        {
            _context.Attach(role);
            _context.Update(role);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Role role)
        {
            _context.Remove(role);
            await _context.SaveChangesAsync();
        }

        public async Task<Role> FindRoleByIdAsync(string id)
        {
            Role role = await RoleSet.FindAsync(id);
            return role;
        }

        public async Task<Role> FindRoleByNameAsync(string roleName)
        {
            Role role = await RoleSet.FirstOrDefaultAsync(r => r.RoleName == roleName);
            return role;
        }

        public async Task<ICollection<User>> GetUsersInRoleAsync(Role role)
        {
            ICollection<User> users = await _context.UserRoles.Include(u => u.User).Where(r => r.RoleId == role.Id).Select(u => u.User).ToListAsync();
            return users;
        }

        public async Task<ICollection<User>> GetUsersNotInRoleAsync(Role role)
        {
            ICollection<User> users = await _context.UserRoles.Include(u => u.User).Where(r => r.RoleId != role.Id).Select(u => u.User).ToListAsync();
            users = users.Distinct().ToList();
            return users;
        }

        public async Task AddUserToRole(User user, Role role)
        {
            UserRole userRole = new UserRole(user, role);
            await _context.AddAsync(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserFromRole(User user, Role role)
        {
            UserRole userRole = await _context.FindAsync<UserRole>(user.Id, role.Id);
            if (userRole == null)
                return;
            _context.Remove(userRole);
            await _context.SaveChangesAsync();
        }
    }
}