using System;
using OMS.AuthZ.Models;

namespace OMS.AuthZ
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public class RequirePermissionAttribute : Attribute
    {
        public Permission RequiredPermission { get; }

        public RequirePermissionAttribute(Permission permission)
        {
            RequiredPermission = permission;
        }
    }
}