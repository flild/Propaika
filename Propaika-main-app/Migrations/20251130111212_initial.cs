using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Propaika_main_app.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeviceModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceModels", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServiceCases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BeforeImage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AfterImage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCompleted = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCases", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ServiceName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPopular = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeviceModelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceItems_DeviceModels_DeviceModelId",
                        column: x => x.DeviceModelId,
                        principalTable: "DeviceModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RepairRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClientName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProblemDescription = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceModelId = table.Column<int>(type: "int", nullable: true),
                    ServiceItemId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsProcessed = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairRequests_DeviceModels_DeviceModelId",
                        column: x => x.DeviceModelId,
                        principalTable: "DeviceModels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RepairRequests_ServiceItems_ServiceItemId",
                        column: x => x.ServiceItemId,
                        principalTable: "ServiceItems",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "DeviceModels",
                columns: new[] { "Id", "DeviceType", "Name" },
                values: new object[,]
                {
                    { 1, 0, "iPhone 13" },
                    { 2, 0, "iPhone 14 Pro" },
                    { 3, 0, "MacBook Air M1" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequests_DeviceModelId",
                table: "RepairRequests",
                column: "DeviceModelId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequests_ServiceItemId",
                table: "RepairRequests",
                column: "ServiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceItems_DeviceModelId",
                table: "ServiceItems",
                column: "DeviceModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepairRequests");

            migrationBuilder.DropTable(
                name: "ServiceCases");

            migrationBuilder.DropTable(
                name: "ServiceItems");

            migrationBuilder.DropTable(
                name: "DeviceModels");
        }
    }
}
