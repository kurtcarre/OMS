using System;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OMS.Auth.Models;
using OMS.Auth.Services;
using OMS.Data;

namespace OMS.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly DBContext dbContext;
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

        public UserController(DBContext _dbContext, ILogger<UserController> _logger, UserManager _userManager)
        {
            dbContext = _dbContext;
            logger = _logger;
            userManager = _userManager;
        }

        public async Task<ActionResult> Index()
        {
            ViewData["Active"] = "users";
            return View(await dbContext.Users.ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(InputModel newUser)
        {
            User user = Activator.CreateInstance<User>();

            user.UserName = newUser.Username;
            user.Email = newUser.Email;
            await userManager.CreateUser(user, newUser.Password);
            logger.LogInformation(LoggerEventIds.UserCreated, "New user created!");
            return RedirectToAction("Index", "User");
        }

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
        public async Task<ActionResult> Edit(User user)
        {
            await userManager.AmendUser(user);
            return RedirectToAction("Index", "User");
        }

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
        public async Task<ActionResult> ResetPassword(ResetModel reset)
        {
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
    }
}