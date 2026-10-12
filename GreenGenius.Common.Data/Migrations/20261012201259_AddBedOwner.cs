using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenGenius.Common.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBedOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_beds_GardenId_Name",
                table: "beds");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "beds",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_beds_GardenId_OwnerId_Name",
                table: "beds",
                columns: new[] { "GardenId", "OwnerId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_beds_GardenId_OwnerId_Name",
                table: "beds");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "beds");

            migrationBuilder.CreateIndex(
                name: "IX_beds_GardenId_Name",
                table: "beds",
                columns: new[] { "GardenId", "Name" });
        }
    }
}
