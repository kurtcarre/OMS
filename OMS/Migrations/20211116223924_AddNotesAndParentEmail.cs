using Microsoft.EntityFrameworkCore.Migrations;

namespace OMS.Migrations
{
    public partial class AddNotesAndParentEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentEmailAddress",
                table: "ChildMembers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "ParentEmailAddress",
                table: "ChildMembers");
        }
    }
}
