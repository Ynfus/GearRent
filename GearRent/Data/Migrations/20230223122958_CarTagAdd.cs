using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearRent.Data.Migrations
{
    /// <inheritdoc />
    public partial class CarTagAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Tag",
                table: "Cars",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tag",
                table: "Cars");
        }
    }
}
