using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganicShopAPI.Migrations
{
    public partial class UpdatedUserIdColumnTOString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCart_AppUsers_AppUserId",
                table: "ShoppingCart");

            migrationBuilder.DropPrimaryKey(name: "PK_AppUsers", table: "AppUsers");

            migrationBuilder.DropIndex(name:"IX_ShoppingCart_AppUserId", table: "ShoppingCart");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "ShoppingCart");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "ShoppingCart",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.DropColumn(
               name: "Id",
               table: "AppUsers");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "AppUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(name: "IX_ShoppingCart_AppUserId", table: "ShoppingCart",
                column: "AppUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCart_AppUsers_AppUserId",
                table: "ShoppingCart");

            migrationBuilder.DropIndex(name: "IX_ShoppingCart_AppUserId", table: "ShoppingCart");

            migrationBuilder.DropPrimaryKey(name: "PK_AppUsers", table: "AppUsers");

            migrationBuilder.DropColumn(
                   name: "AppUserId",
                   table: "ShoppingCart");

            migrationBuilder.AddColumn<int>(
                name: "AppUserId",
                table: "ShoppingCart",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.DropColumn(
                   name: "Id",
                   table: "AppUsers");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AppUsers",
                type: "int",
                nullable: false).Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(name: "PK_AppUsers", table: "AppUsers", column: "Id");

            migrationBuilder.CreateIndex(name: "IX_ShoppingCart_AppUserId", table: "ShoppingCart",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCart_AppUsers_AppUserId",
                table: "ShoppingCart",
                column: "AppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
