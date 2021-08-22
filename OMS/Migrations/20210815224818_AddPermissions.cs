using Microsoft.EntityFrameworkCore.Migrations;

namespace OMS.Migrations
{
    public partial class AddPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AdminPermissions",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Admin_RolePermission",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Admin_UserPermission",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChildMemberPermission",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MemberPermission",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminPermissions",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Admin_RolePermission",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Admin_UserPermission",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ChildMemberPermission",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "MemberPermission",
                table: "Roles");
        }
    }
}
