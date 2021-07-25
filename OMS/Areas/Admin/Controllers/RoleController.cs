using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Auth.Models;
using OMS.Auth.Services;
using OMS.Data;

namespace OMS.Admin.Controllers
{
    [Area("Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager roleManager;

        public RoleController(RoleManager _roleManager)
        {
            roleManager = _roleManager;
        }

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

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Role role)
        {
            await roleManager.CreateAsync(role);
            return RedirectToAction("Index", "Role");
        }

        public async Task<ActionResult> Edit(string Id)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            Role role = await roleManager.FindRoleByIdAsync(Id);
            if (role == null)
                return NotFound();

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string Id, Role role)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            await roleManager.AmendAsync(role);
            return RedirectToAction("Index", "Role");
        }

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

        public async Task<ActionResult> ManageUsers(string Id)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            Role role = await roleManager.FindRoleByIdAsync(Id);
            role.Users = await roleManager.GetUsersInRoleAsync(role);
            return View(new ManageUserModel(role.Id, role.Users, await roleManager.GetUsersNotInRoleAsync(role)));
        }

        public class ManageUserModel
        {
            public string RoleId { get; set; }

            public ICollection<User> RoleUsers { get; set; }
            public ICollection<User> OtherUsers { get; set; }

            public ManageUserModel(string roleId, ICollection<User> roleUsers, ICollection<User> otherUsers)
            {
                RoleId = roleId;
                RoleUsers = roleUsers;
                OtherUsers = otherUsers;
            }
        }
    }
}