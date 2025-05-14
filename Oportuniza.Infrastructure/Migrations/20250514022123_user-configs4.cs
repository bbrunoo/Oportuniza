using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oportuniza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class userconfigs4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "User");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEnd",
                table: "User",
                type: "datetime2",
                nullable: true);
        }
    }
}
