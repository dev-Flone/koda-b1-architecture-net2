using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KodaStore.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProductAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAtUtc",
                schema: "dbo",
                table: "Product",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Product",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeactivatedAtUtc",
                schema: "dbo",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Product");
        }
    }
}
