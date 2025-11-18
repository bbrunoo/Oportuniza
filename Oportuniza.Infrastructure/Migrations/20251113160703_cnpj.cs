using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Oportuniza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class cnpj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("74a29f44-f930-4001-b037-46ea774ec2d9"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("c251b72e-f584-44e3-97ba-c027ffdf2e03"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("fcd83901-472b-4fad-894e-301bbbc57098"));

            migrationBuilder.CreateTable(
                name: "CnpjCache",
                columns: table => new
                {
                    Cnpj = table.Column<string>(type: "char(14)", nullable: false),
                    Situacao = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CnpjCache", x => x.Cnpj);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CnpjCache");

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

            migrationBuilder.InsertData(
                table: "CompanyRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("74a29f44-f930-4001-b037-46ea774ec2d9"), "Administrator" },
                    { new Guid("c251b72e-f584-44e3-97ba-c027ffdf2e03"), "Worker" },
                    { new Guid("fcd83901-472b-4fad-894e-301bbbc57098"), "Owner" }
                });
        }
    }
}
