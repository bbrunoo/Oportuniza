using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Oportuniza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class local : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("0b126768-c816-4c17-b58f-6ae1636777a5"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("60081819-eb25-4649-9db8-516f7a86f0a7"));

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "User",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Local",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "User",
                type: "float",
                nullable: true);

            migrationBuilder.InsertData(
                table: "CompanyRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("006ea434-8d6e-4b95-91ec-ad0d034299fa"), "Administrator" },
                    { new Guid("a5433110-793e-4723-a636-405b45929c9a"), "Worker" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("006ea434-8d6e-4b95-91ec-ad0d034299fa"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("a5433110-793e-4723-a636-405b45929c9a"));

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Local",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "User");

            migrationBuilder.InsertData(
                table: "CompanyRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("0b126768-c816-4c17-b58f-6ae1636777a5"), "Worker" },
                    { new Guid("60081819-eb25-4649-9db8-516f7a86f0a7"), "Administrator" }
                });
        }
    }
}
