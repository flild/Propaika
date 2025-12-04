using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Propaika_main_app.Migrations
{
    /// <inheritdoc />
    public partial class addmigrationv4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.AddColumn<string>(
                name: "DeviceModel",
                table: "ServiceCases",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "ServiceCases",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "ServiceCases",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "ServiceCases",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "ServiceCases",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_Slug",
                table: "ServiceCases",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceCases_Slug",
                table: "ServiceCases");

            migrationBuilder.DropColumn(
                name: "DeviceModel",
                table: "ServiceCases");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "ServiceCases");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "ServiceCases");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "ServiceCases");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "ServiceCases");

            migrationBuilder.InsertData(
                table: "DeviceModels",
                columns: new[] { "Id", "DeviceType", "Name" },
                values: new object[,]
                {
                    { 1, 0, "iPhone 13" },
                    { 2, 0, "iPhone 14 Pro" },
                    { 3, 0, "MacBook Air M1" }
                });
        }
    }
}
