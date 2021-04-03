using Microsoft.EntityFrameworkCore.Migrations;

namespace OMS.Migrations
{
    public partial class AddChildMembers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Members",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Section",
                table: "Members",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MemberType",
                table: "Members",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Members",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Members",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChildMemberMemberNo",
                table: "Members",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChildMembers",
                columns: table => new
                {
                    MemberNo = table.Column<int>(type: "int", nullable: false),
                    ParentFirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentLastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmergencyContactNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Consent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildMembers", x => x.MemberNo);
                    table.ForeignKey(
                        name: "FK_ChildMembers_Members_MemberNo",
                        column: x => x.MemberNo,
                        principalTable: "Members",
                        principalColumn: "MemberNo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Members_ChildMemberMemberNo",
                table: "Members",
                column: "ChildMemberMemberNo");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_ChildMembers_ChildMemberMemberNo",
                table: "Members",
                column: "ChildMemberMemberNo",
                principalTable: "ChildMembers",
                principalColumn: "MemberNo",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_ChildMembers_ChildMemberMemberNo",
                table: "Members");

            migrationBuilder.DropTable(
                name: "ChildMembers");

            migrationBuilder.DropIndex(
                name: "IX_Members_ChildMemberMemberNo",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "ChildMemberMemberNo",
                table: "Members");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Section",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MemberType",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Members",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Members",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
