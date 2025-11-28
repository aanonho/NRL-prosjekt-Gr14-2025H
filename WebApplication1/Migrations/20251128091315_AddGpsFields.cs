using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddGpsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ObstacleLatitude",
                table: "ReportItem",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ObstacleLongitude",
                table: "ReportItem",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObstacleLatitude",
                table: "ReportItem");

            migrationBuilder.DropColumn(
                name: "ObstacleLongitude",
                table: "ReportItem");
        }
    }
}
