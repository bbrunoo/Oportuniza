using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Oportuniza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Roles",
                table: "CompanyEmployee");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyRoleId",
                table: "CompanyEmployee",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "CompanyRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyRole", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CompanyRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("03338af4-2b09-41a5-bc0e-f72a73b12f47"), "Administrator" },
                    { new Guid("958d58e4-8fa7-4fb4-8afe-ca142b8e82a4"), "Worker" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyEmployee_CompanyRoleId",
                table: "CompanyEmployee",
                column: "CompanyRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyEmployee_CompanyRole_CompanyRoleId",
                table: "CompanyEmployee",
                column: "CompanyRoleId",
                principalTable: "CompanyRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyEmployee_CompanyRole_CompanyRoleId",
                table: "CompanyEmployee");

            migrationBuilder.DropTable(
                name: "CompanyRole");

            migrationBuilder.DropIndex(
                name: "IX_CompanyEmployee_CompanyRoleId",
                table: "CompanyEmployee");

            migrationBuilder.DropColumn(
                name: "CompanyRoleId",
                table: "CompanyEmployee");

            migrationBuilder.AddColumn<string>(
                name: "Roles",
                table: "CompanyEmployee",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
