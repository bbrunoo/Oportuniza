using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Oportuniza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class post : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("01bd9cf9-c209-4b00-a875-474cdbc940df"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("0be93db8-fe73-470b-9f2e-c5f5d8d7258b"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("772a8f7b-ade5-4412-a4c6-7590027c524d"));

            migrationBuilder.AddColumn<string>(
                name: "PostAuthorName",
                table: "Publication",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "CompanyRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("2e535245-ad07-4782-9a49-03cb993db14e"), "Administrator" },
                    { new Guid("3af04d20-f161-4ff2-8949-203c73d036b7"), "Owner" },
                    { new Guid("9fcd17bd-3829-4031-a0f4-e9124cda973e"), "Worker" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("2e535245-ad07-4782-9a49-03cb993db14e"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("3af04d20-f161-4ff2-8949-203c73d036b7"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("9fcd17bd-3829-4031-a0f4-e9124cda973e"));

            migrationBuilder.DropColumn(
                name: "PostAuthorName",
                table: "Publication");

            migrationBuilder.InsertData(
                table: "CompanyRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("01bd9cf9-c209-4b00-a875-474cdbc940df"), "Owner" },
                    { new Guid("0be93db8-fe73-470b-9f2e-c5f5d8d7258b"), "Administrator" },
                    { new Guid("772a8f7b-ade5-4412-a4c6-7590027c524d"), "Worker" }
                });
        }
    }
}
