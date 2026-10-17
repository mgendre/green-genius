using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenGenius.Common.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantAssociations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "plant_associations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlantAId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlantBId = table.Column<Guid>(type: "uuid", nullable: false),
                    Effect = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Evidence = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NoteFr = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plant_associations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_plant_associations_plants_PlantAId",
                        column: x => x.PlantAId,
                        principalTable: "plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_plant_associations_plants_PlantBId",
                        column: x => x.PlantBId,
                        principalTable: "plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plant_association_mechanisms",
                columns: table => new
                {
                    PlantAssociationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mechanism = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plant_association_mechanisms", x => new { x.PlantAssociationId, x.Mechanism });
                    table.ForeignKey(
                        name: "FK_plant_association_mechanisms_plant_associations_PlantAssoci~",
                        column: x => x.PlantAssociationId,
                        principalTable: "plant_associations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_plant_associations_PlantAId_PlantBId",
                table: "plant_associations",
                columns: new[] { "PlantAId", "PlantBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plant_associations_PlantBId",
                table: "plant_associations",
                column: "PlantBId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plant_association_mechanisms");

            migrationBuilder.DropTable(
                name: "plant_associations");
        }
    }
}
