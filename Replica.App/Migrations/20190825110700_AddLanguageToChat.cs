using Microsoft.EntityFrameworkCore.Migrations;

namespace Replica.App.Migrations
{
    public partial class AddLanguageToChat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Chats",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Notifications",
                table: "Chats",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "Notifications",
                table: "Chats");
        }
    }
}
