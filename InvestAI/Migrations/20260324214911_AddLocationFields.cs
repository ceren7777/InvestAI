using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestAI.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Regions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Regions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PopulationUpdatedAt",
                table: "Regions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "PopulationUpdatedAt",
                table: "Regions");
        }
    }
}
