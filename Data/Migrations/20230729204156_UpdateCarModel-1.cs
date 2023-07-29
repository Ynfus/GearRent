using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearRent.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCarModel1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Acceleration",
                table: "Cars",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "FuelConsumption",
                table: "Cars",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "FuelType",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Acceleration",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "FuelConsumption",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "FuelType",
                table: "Cars");
        }
    }
}
