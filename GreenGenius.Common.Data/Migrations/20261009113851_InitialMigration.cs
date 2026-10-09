using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenGenius.Common.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gardens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gardens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "plants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameFr = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DescriptionFr = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    BinomialName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Family = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LifeCycle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DaysToMaturity = table.Column<int>(type: "integer", nullable: true),
                    TemperatureMinC = table.Column<int>(type: "integer", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "plant_needs",
                columns: table => new
                {
                    PlantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sunlight = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WaterNeed = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RootDepth = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SoilPhMin = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    SoilPhMax = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    SpacingRowCm = table.Column<int>(type: "integer", nullable: false),
                    SpacingPlantCm = table.Column<int>(type: "integer", nullable: false),
                    HeightCm = table.Column<int>(type: "integer", nullable: false),
                    SpreadCm = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plant_needs", x => x.PlantId);
                    table.ForeignKey(
                        name: "FK_plant_needs_plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plant_traits",
                columns: table => new
                {
                    PlantId = table.Column<Guid>(type: "uuid", nullable: false),
                    NitrogenFixer = table.Column<bool>(type: "boolean", nullable: false),
                    DynamicAccumulator = table.Column<bool>(type: "boolean", nullable: false),
                    PollinatorFriendly = table.Column<bool>(type: "boolean", nullable: false),
                    DroughtTolerant = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plant_traits", x => x.PlantId);
                    table.ForeignKey(
                        name: "FK_plant_traits_plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gardens_OwnerId_Name",
                table: "gardens",
                columns: new[] { "OwnerId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_plants_Key",
                table: "plants",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gardens");

            migrationBuilder.DropTable(
                name: "plant_needs");

            migrationBuilder.DropTable(
                name: "plant_traits");

            migrationBuilder.DropTable(
                name: "plants");
        }
    }
}
