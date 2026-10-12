using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenGenius.Common.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantEmoji : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "plants",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "plants");
        }
    }
}
