using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OMS.Auth.Models;
using OMS.Auth.Services;
using OMS.AuthZ;
using OMS.AuthZ.Models;

namespace OMS.Admin.Controllers
{
    [Area("Admin")]
    [PermissionType(PermissionType.Admin_Users)]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> logger;
        private readonly UserManager userManager;

        public class InputModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "Passwords don't match!")]
            public string ConfirmPassword { get; set; }
        }

        public UserController(ILogger<UserController> _logger, UserManager _userManager)
        {
            logger = _logger;
            userManager = _userManager;
        }

        [RequirePermission(Permission.Read)]
        public async Task<ActionResult> Index()
        {
            ViewData["Active"] = "users";
            return View(await userManager.Users.ToListAsync());
        }

        [RequirePermission(Permission.Create)]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.Create)]
        public async Task<ActionResult> Create(InputModel newUser)
        {
            User user = Activator.CreateInstance<User>();

            user.UserName = newUser.Username;
            user.Email = newUser.Email;
            await userManager.CreateUser(user, newUser.Password);
            logger.LogInformation(LoggerEventIds.UserCreated, "New user created!");
            return RedirectToAction("Index", "User");
        }

        [RequirePermission(Permission.Write)]
        public async Task<ActionResult> Edit(string Id)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            User user = await userManager.FindUserById(Id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.Write)]
        public async Task<ActionResult> Edit(string Id, User user)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            await userManager.AmendUser(user);
            return RedirectToAction("Index", "User");
        }

        [RequirePermission(Permission.Write)]
        public ActionResult ResetPassword(string Id)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            ResetModel reset = new ResetModel
            {
                Id = Id
            };

            return View(reset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.Write)]
        public async Task<ActionResult> ResetPassword(string Id, ResetModel reset)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            User user = await userManager.FindUserById(reset.Id);

            if (user == null)
                return NotFound();

            await userManager.AdminResetPasswordAsync(user, reset.NewPassword);
            return RedirectToAction("Index", "User");
        }

        public class ResetModel
        {
            [Required]
            public string Id { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("NewPassword", ErrorMessage = "Passwords must match!")]
            public string ConfirmPassword { get; set; }
        }

        [RequirePermission(Permission.Full)]
        public async Task<ActionResult> Delete(string Id)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            User user = await userManager.FindUserById(Id);
            if (user == null)
                return NotFound();

            if (user.Id == User.FindFirst("UserID").Value)
                return Forbid();

            await userManager.DeleteUser(user);
            return RedirectToAction("Index", "User");
        }
    }
}