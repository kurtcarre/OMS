using System;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OMS.Auth;
using OMS.Data;

namespace OMS.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly DBContext dbContext;
        private readonly ILogger<UserController> logger;
        private readonly UserManager<User> userManager;
        private readonly IUserStore<User> userStore;

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

        public UserController(DBContext _dbContext, ILogger<UserController> _logger, UserManager<User> _userManager, IUserStore<User> _userStore)
        {
            dbContext = _dbContext;
            logger = _logger;
            userManager = _userManager;
            userStore = _userStore;
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

            await userStore.SetUserNameAsync(user, newUser.Username, CancellationToken.None);
            IUserEmailStore<User> emailStore = userStore as IUserEmailStore<User>;
            await emailStore.SetEmailAsync(user, newUser.Email, CancellationToken.None);
            AuthResult result = await userManager.CreateAsync(user, newUser.Password);
            if(result.Succeeded)
            {
                logger.LogInformation(LoggerEventIds.UserCreated, "New user created!");
                return RedirectToAction("Index", "User");
            }

            ViewData["Error"] = result.ToString();
            return View();
        }

        public async Task<ActionResult> Edit(string Id)
        {
            if (Id == null || Id == string.Empty)
                return NotFound();

            User user = await userStore.FindByIdAsync(Id, CancellationToken.None);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(User user)
        {
            AuthResult result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
                return RedirectToAction("Index", "User");

            ViewData["Error"] = result.ToString();
            return View();
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
            User user = await userStore.FindByIdAsync(reset.Id, CancellationToken.None);

            if (user == null)
                return NotFound();

            AuthResult result = await userManager.AdminResetPasswordAsync(user, reset.NewPassword);
            if (result.Succeeded)
                return RedirectToAction("Index", "User");

            ViewData["Error"] = result.ToString();
            return View();
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