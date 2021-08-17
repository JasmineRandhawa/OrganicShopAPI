using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganicShopAPI.Migrations
{
    public partial class AddedPriceColumnToProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("UPDATE dbo.Products SET PRICE = 5.2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Products");
        }
    }
}
