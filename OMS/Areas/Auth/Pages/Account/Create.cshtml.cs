using System;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OMS.Auth.Models;
using OMS.Auth.Services;

namespace OMS.Auth.UI.Pages
{
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public class CreateModel : PageModel
    {
        private readonly SignInManager SignInManager;
        private readonly UserManager UserManager;
        private readonly ILogger<CreateModel> Logger;

        public string ReturnUrl { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

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
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "Passwords don't match!")]
            public string ConfirmPassword { get; set; }
        }

        public CreateModel(SignInManager signInManager, UserManager userManager, ILogger<CreateModel> logger)
        {
            SignInManager = signInManager;
            UserManager = userManager;
            Logger = logger;
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            if(ModelState.IsValid)
            {
                User user = Activator.CreateInstance<User>();

                user.UserName = Input.Username;
                user.Email = Input.Email;
                await UserManager.CreateUser(user, Input.Password);
                Logger.LogInformation(LoggerEventIds.UserCreated, "New user created!");
                await SignInManager.SignInAsync(user);
                if (returnUrl == null)
                    return LocalRedirect("~/");
                else
                    return LocalRedirect(returnUrl);
            }
            return Page();
        }
    }
}