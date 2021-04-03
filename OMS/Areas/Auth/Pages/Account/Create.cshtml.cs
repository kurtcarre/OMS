using System;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace OMS.Auth.UI.Pages
{
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public class CreateModel : PageModel
    {
        private readonly SignInManager<User> SignInManager;
        private readonly UserManager<User> UserManager;
        private readonly ILogger<CreateModel> Logger;
        private readonly IUserStore<User> UserStore;

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

        public CreateModel(SignInManager<User> signInManager, UserManager<User> userManager, ILogger<CreateModel> logger, IUserStore<User> userStore)
        {
            SignInManager = signInManager;
            UserManager = userManager;
            Logger = logger;
            UserStore = userStore;
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

                await UserStore.SetUserNameAsync(user, Input.Username, CancellationToken.None);
                IUserEmailStore<User> emailStore = UserStore as IUserEmailStore<User>;
                await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                AuthResult result = await UserManager.CreateAsync(user, Input.Password);
                if(result.Succeeded)
                {
                    Logger.LogInformation(LoggerEventIds.UserCreated, "New user created!");

                    if(UserManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        // DEV ONLY
                        await emailStore.SetEmailConfirmedAsync(user, true, CancellationToken.None);
                        await SignInManager.SignInAsync(user, false);
                        if (returnUrl == null)
                            return LocalRedirect("~/");
                        else
                            return LocalRedirect(returnUrl);
                    }
                }
            }
            return Page();
        }
    }
}