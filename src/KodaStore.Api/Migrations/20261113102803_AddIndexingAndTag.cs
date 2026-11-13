using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KodaStore.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexingAndTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Product_CustomerId",
                schema: "dbo",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderDate",
                schema: "dbo",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CustomerId_Price_Category",
                schema: "dbo",
                table: "Product",
                columns: new[] { "CustomerId", "Price", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber_OrderDate_Status",
                schema: "dbo",
                table: "Orders",
                columns: new[] { "OrderNumber", "OrderDate", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CustomerId_Price_Category",
                schema: "dbo",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderNumber_OrderDate_Status",
                schema: "dbo",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CustomerId",
                schema: "dbo",
                table: "Product",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate",
                schema: "dbo",
                table: "Orders",
                column: "OrderDate");
        }
    }
}
