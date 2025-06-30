using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oportuniza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class publication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Publication",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Publication");
        }
    }
}
