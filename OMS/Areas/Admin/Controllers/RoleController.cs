using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Auth.Models;
using OMS.Auth.Services;
using OMS.AuthZ;
using OMS.AuthZ.Models;

namespace OMS.Admin.Controllers
{
    [Area("Admin")]
    [PermissionType(PermissionType.Admin_Roles)]
    public class RoleController : Controller
    {
        private readonly RoleManager roleManager;
        private readonly UserManager userManager;

        public RoleController(RoleManager _roleManager, UserManager _userManager)
        {
            roleManager = _roleManager;
            userManager = _userManager;
        }

        [RequirePermission(Permission.Read)]
        public async Task<ActionResult> Index()
        {
            ViewData["Active"] = "roles";
            ICollection<Role> roles = await roleManager.Roles.ToListAsync();
            foreach(var role in roles)
            {
                ICollection<User> users = await roleManager.GetUsersInRoleAsync(role);
                role.Users = users;
            }
            return View(roles);
        }

        [RequirePermission(Permission.Create)]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.Create)]
        public async Task<ActionResult> Create(Role role)
        {
            await roleManager.CreateAsync(role);
            return RedirectToAction("Index", "Role");
        }

        [RequirePermission(Permission.Write)]
        public async Task<ActionResult> Edit(string Id)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            Role role = await roleManager.FindRoleByIdAsync(Id);
            if (role == null)
                return NotFound();

            role.MemberPermissionString = role.MemberPermission.ToString();
            role.ChildMemberPermissionString = role.ChildMemberPermission.ToString();
            role.Admin_UserPermissionString = role.Admin_UserPermission.ToString();
            role.Admin_RolePermissionString = role.Admin_RolePermission.ToString();

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.Write)]
        public async Task<ActionResult> Edit(string Id, Role role)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            role.MemberPermission = Convert.ToInt32(role.MemberPermissionString);
            role.ChildMemberPermission = Convert.ToInt32(role.ChildMemberPermissionString);
            role.Admin_UserPermission = Convert.ToInt32(role.Admin_UserPermissionString);
            role.Admin_RolePermission = Convert.ToInt32(role.Admin_RolePermissionString);

            await roleManager.AmendAsync(role);
            return RedirectToAction("Index", "Role");
        }

        [RequirePermission(Permission.Full)]
        public async Task<ActionResult> Delete(string Id)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            Role role = await roleManager.FindRoleByIdAsync(Id);
            if (role == null)
                return NotFound();

            await roleManager.DeleteAsync(role);
            return RedirectToAction("Index", "Role");
        }

        [RequirePermission(Permission.Write)]
        public async Task<ActionResult> ManageUsers(string Id)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            Role role = await roleManager.FindRoleByIdAsync(Id);
            role.Users = await roleManager.GetUsersInRoleAsync(role);
            return View(new ManageUserModel(role.Id, role.RoleName, role.Users, await roleManager.GetUsersNotInRoleAsync(role)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.Write)]
        public async Task<ActionResult> ManageUsers(string Id, ManageUserModel model)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            Role role = await roleManager.FindRoleByIdAsync(Id);
            if (role == null)
                return NotFound();

            if (model.AddUsers != null)
            {
                foreach (var id in model.AddUsers)
                {
                    User user = await userManager.FindUserById(id);
                    await roleManager.AddUserToRole(user, role);
                }
            }

            if (model.RemoveUsers != null)
            {
                foreach (var id in model.RemoveUsers)
                {
                    User user = await userManager.FindUserById(id);
                    await roleManager.RemoveUserFromRole(user, role);
                }
            }

            return RedirectToAction("Index", "Role");
        }

        public class ManageUserModel
        {
            public string RoleId { get; set; }
            public string RoleName { get; set; }

            public ICollection<User> RoleUsers { get; set; }
            public ICollection<User> OtherUsers { get; set; }

            public ICollection<string> AddUsers { get; set; }
            public ICollection<string> RemoveUsers { get; set; }

            public ManageUserModel()
            {

            }

            public ManageUserModel(string roleId, string roleName, ICollection<User> roleUsers, ICollection<User> otherUsers)
            {
                RoleId = roleId;
                RoleName = roleName;
                RoleUsers = roleUsers;
                OtherUsers = otherUsers;
            }
        }
    }
}