using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganicShopAPI.Migrations
{
    public partial class AddedDateModifiedToShoppingCart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DateModified",
                table: "ShoppingCart",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "ShoppingCart");
        }
    }
}
