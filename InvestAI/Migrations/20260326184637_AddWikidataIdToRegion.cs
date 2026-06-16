using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestAI.Migrations
{
    /// <inheritdoc />
    public partial class AddWikidataIdToRegion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WikidataId",
                table: "Regions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WikidataId",
                table: "Regions");
        }
    }
}
