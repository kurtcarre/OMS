using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;
using OMS.AuthZ.Models;

namespace OMS.Auth.Models
{
    public class Role
    {
        public string Id { get; set; }
        [Display(Name = "Role name")]
        public string RoleName { get; set; }

        public virtual ICollection<User> Users { get; set; }

        public int MemberPermission { get; set; }
        public int ChildMemberPermission { get; set; }

        [Display(Name = "Has admin permissions?")]
        public bool AdminPermissions { get; set; }
        public int Admin_UserPermission { get; set; }
        public int Admin_RolePermission { get; set; }

        [NotMapped]
        [Display(Name = "Member Permissions")]
        public string MemberPermissionString { get; set; }
        [NotMapped]
        [Display(Name = "Child Member Permissions")]
        public string ChildMemberPermissionString { get; set; }
        [NotMapped]
        [Display(Name = "User Permissions")]
        public string Admin_UserPermissionString { get; set; }
        [NotMapped]
        [Display(Name = "Role permissions")]
        public string Admin_RolePermissionString { get; set; }

        public Role()
        {
            Id = Guid.NewGuid().ToString();
            AdminPermissions = false;
            MemberPermission = ChildMemberPermission = Admin_UserPermission = Admin_RolePermission = 0;
        }

        public Role(string name) : this()
        {
            RoleName = name;
        }

        public override string ToString() => RoleName;

        public Permission GetPermission(string permissionType)
        {
            switch(permissionType)
            {
                case PermissionType.Members: return (Permission)MemberPermission;
                case PermissionType.ChildMembers: return (Permission)ChildMemberPermission;
                case PermissionType.Admin_Users: return AdminPermissions ? (Permission)Admin_UserPermission : Permission.None;
                case PermissionType.Admin_Roles: return AdminPermissions ? (Permission)Admin_RolePermission : Permission.None;
                default: return Permission.None;
            }
        }

        public static List<SelectListItem> PermissionValues() => new List<SelectListItem>(new SelectListItem[]
            {
                new SelectListItem("None", "0"),
                new SelectListItem("Read", "1"),
                new SelectListItem("Write", "2"),
                new SelectListItem("Create", "3"),
                new SelectListItem("Full Access", "4")
            });
    }
}