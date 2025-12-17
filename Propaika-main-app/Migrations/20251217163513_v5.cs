using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Propaika_main_app.Migrations
{
    /// <inheritdoc />
    public partial class v5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProcessed",
                table: "RepairRequests");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "RepairRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "RepairRequests");

            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                table: "RepairRequests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
