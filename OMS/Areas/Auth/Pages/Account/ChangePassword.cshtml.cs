using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Auth.Services;

namespace OMS.Auth.UI.Pages
{
    [ValidateAntiForgeryToken]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager userManager;

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Current password")]
            [DataType(DataType.Password)]
            public string CurrentPassword { get; set; }

            [Required]
            [Display(Name = "New password")]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "Passwords must match!")]
            public string ConfirmNewPassword { get; set; }
        }

        public ChangePasswordModel(UserManager _userManager)
        {
            userManager = _userManager;
        }

        public IActionResult OnGetAsync(string id = null)
        {
            if (id == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id = null)
        {
            var user = await userManager.FindUserById(id);

            if (!await userManager.CheckPasswordAsync(user, Input.CurrentPassword))
                return Forbid();

            await userManager.ResetPasswordAsync(user, Input.NewPassword);
            return LocalRedirect("/");
        }
    }
}