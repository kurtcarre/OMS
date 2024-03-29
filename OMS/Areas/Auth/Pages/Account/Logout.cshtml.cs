﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OMS.Auth.Services;

namespace OMS.Auth.UI.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager SignInManager;
        private readonly ILogger<LogoutModel> Logger;

        public LogoutModel(SignInManager signInManager, ILogger<LogoutModel> logger)
        {
            SignInManager = signInManager;
            Logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                await SignInManager.SignOutAsync();
                Logger.LogInformation(LoggerEventIds.UserLoggedOut, "User logged out");
            }
            return Page();
        }
    }
}