using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenGenius.Common.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamePlantFrostToleranceDropDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysToMaturity",
                table: "plants");

            migrationBuilder.RenameColumn(
                name: "TemperatureMinC",
                table: "plants",
                newName: "FrostToleranceC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FrostToleranceC",
                table: "plants",
                newName: "TemperatureMinC");

            migrationBuilder.AddColumn<int>(
                name: "DaysToMaturity",
                table: "plants",
                type: "integer",
                nullable: true);
        }
    }
}
