using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OMS.Auth.Services;
using OMS.AuthZ.Models;

namespace OMS.AuthZ
{
    public class OMSAuthorisationMiddleware
    {
        private readonly RequestDelegate next;

        public OMSAuthorisationMiddleware(RequestDelegate _next)
        {
            next = _next;
        }

        public async Task Invoke(HttpContext context, UserManager userManager)
        {
            var endpoint = context.GetEndpoint();

            var anonymous = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>();

            // Skip authz if allow anonymous is set
            if(anonymous != null)
            {
                await next(context);
                return;
            }

            // Require logged in user
            if(context.User.Identity.IsAuthenticated == false)
            {
                await context.ChallengeAsync("OMS.Auth");
                return;
            }

            var permissionType = endpoint?.Metadata.GetMetadata<PermissionTypeAttribute>()?.PermissionType;

            if(permissionType == null)
            {
                await next(context);
                return;
            }

            var requirement = endpoint?.Metadata.GetMetadata<RequirePermissionAttribute>()?.RequiredPermission;

            if(requirement == null)
            {
                await next(context);
                return;
            }

            var user = await userManager.FindUserById(context.User.FindFirst("UserID").Value);

            var usersPermissions = userManager.GetEffectivePermission(user, permissionType);

            if((int)usersPermissions >= (int)requirement)
            {
                await next(context);
                return;
            }
            else
            {
                await context.ForbidAsync();
                return;
            }
        }
    }
}