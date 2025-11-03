using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Oportuniza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class verified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("006ea434-8d6e-4b95-91ec-ad0d034299fa"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("a5433110-793e-4723-a636-405b45929c9a"));

            migrationBuilder.AddColumn<bool>(
                name: "VerifiedEmail",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "CompanyRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("d9e4c6a0-43e0-4568-bafc-265b4b47621b"), "Worker" },
                    { new Guid("e28fc01b-c6e6-4a4a-9e25-131d1f7b9e96"), "Administrator" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("d9e4c6a0-43e0-4568-bafc-265b4b47621b"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("e28fc01b-c6e6-4a4a-9e25-131d1f7b9e96"));

            migrationBuilder.DropColumn(
                name: "VerifiedEmail",
                table: "User");

            migrationBuilder.InsertData(
                table: "CompanyRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("006ea434-8d6e-4b95-91ec-ad0d034299fa"), "Administrator" },
                    { new Guid("a5433110-793e-4723-a636-405b45929c9a"), "Worker" }
                });
        }
    }
}
