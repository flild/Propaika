using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Propaika_main_app.Migrations
{
    /// <inheritdoc />
    public partial class v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairRequests_DeviceModels_DeviceModelId",
                table: "RepairRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairRequests_ServiceItems_ServiceItemId",
                table: "RepairRequests");

            migrationBuilder.DropIndex(
                name: "IX_RepairRequests_DeviceModelId",
                table: "RepairRequests");

            migrationBuilder.DropIndex(
                name: "IX_RepairRequests_ServiceItemId",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "DeviceModelId",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "ServiceItemId",
                table: "RepairRequests");

            migrationBuilder.AddColumn<string>(
                name: "DeviceModel",
                table: "RepairRequests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ServiceItem",
                table: "RepairRequests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceModel",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "ServiceItem",
                table: "RepairRequests");

            migrationBuilder.AddColumn<int>(
                name: "DeviceModelId",
                table: "RepairRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceItemId",
                table: "RepairRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequests_DeviceModelId",
                table: "RepairRequests",
                column: "DeviceModelId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequests_ServiceItemId",
                table: "RepairRequests",
                column: "ServiceItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairRequests_DeviceModels_DeviceModelId",
                table: "RepairRequests",
                column: "DeviceModelId",
                principalTable: "DeviceModels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairRequests_ServiceItems_ServiceItemId",
                table: "RepairRequests",
                column: "ServiceItemId",
                principalTable: "ServiceItems",
                principalColumn: "Id");
        }
    }
}
