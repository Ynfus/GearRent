using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearRent.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatereservationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ReservationValue",
                table: "Reservations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationValue",
                table: "Reservations");
        }
    }
}
