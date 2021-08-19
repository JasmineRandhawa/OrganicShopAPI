using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganicShopAPI.Migrations
{
    public partial class UpdateAppUserNameColumnInAppUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AppUsers",
                newName: "AppUserName");

            migrationBuilder.AddColumn<string>(
                name: "AppUserName",
                table: "ShoppingCart",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppUserName",
                table: "ShoppingCart");

            migrationBuilder.RenameColumn(
                name: "AppUserName",
                table: "AppUsers",
                newName: "Name");
        }
    }
}
