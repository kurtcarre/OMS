using System;
using OMS.AuthZ.Models;

namespace OMS.AuthZ
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PermissionTypeAttribute : Attribute
    {
        public string PermissionType { get; }

        public PermissionTypeAttribute(string type)
        {
            PermissionType = type;
        }
    }
}