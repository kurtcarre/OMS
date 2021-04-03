using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OMS.Migrations
{
    public partial class AddChildDOB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DoB",
                table: "ChildMembers",
                type: "datetime2",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoB",
                table: "ChildMembers");
        }
    }
}
