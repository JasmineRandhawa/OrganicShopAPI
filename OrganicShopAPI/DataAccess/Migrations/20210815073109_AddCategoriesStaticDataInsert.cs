using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganicShopAPI.Migrations
{
    public partial class AddCategoriesStaticDataInsert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isActive",
                table: "Categories",
                newName: "IsActive");

            migrationBuilder.Sql("INSERT INTO Categories(Name,IsActive) VALUES('Bakery',1)");
            migrationBuilder.Sql("INSERT INTO Categories(Name,IsActive) VALUES('Fruits',1)");
            migrationBuilder.Sql("INSERT INTO Categories(Name,IsActive) VALUES('Vegetables',1)");
            migrationBuilder.Sql("INSERT INTO Categories(Name,IsActive) VALUES('Spices',1)");
            migrationBuilder.Sql("INSERT INTO Categories(Name,IsActive) VALUES('Dairy',1)");
            migrationBuilder.Sql("INSERT INTO Categories(Name,IsActive) VALUES('SoftDrinks',1)");
            migrationBuilder.Sql("INSERT INTO Categories(Name,IsActive) VALUES('Beverages',0)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Categories",
                newName: "isActive");
        }
    }
}
