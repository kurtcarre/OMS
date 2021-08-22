using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OMS.Auth.Services;

namespace OMS.Auth.UI.Pages
{
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public class LoginModel : PageModel
    {
        private readonly SignInManager SignInManager;
        private readonly ILogger<LoginModel> Logger;

        public LoginModel(SignInManager signInManager, ILogger<LoginModel> logger)
        {
            SignInManager = signInManager;
            Logger = logger;
        }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public void OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            var result = await SignInManager.PasswordSignInAsync(Input.Username, Input.Password);
            if(result == Services.SignInResult.Success)
            {
                Logger.LogInformation(LoggerEventIds.UserLoggedIn, "User logged in!");
                if (returnUrl == null)
                    return LocalRedirect("~/");

                return LocalRedirect(returnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt!");
                return Page();
            }
        }
    }
}