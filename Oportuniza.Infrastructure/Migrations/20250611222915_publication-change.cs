using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oportuniza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class publicationchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorType",
                table: "Publication");

            migrationBuilder.CreateIndex(
                name: "IX_Publication_AuthorId",
                table: "Publication",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Publication_User_AuthorId",
                table: "Publication",
                column: "AuthorId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publication_User_AuthorId",
                table: "Publication");

            migrationBuilder.DropIndex(
                name: "IX_Publication_AuthorId",
                table: "Publication");

            migrationBuilder.AddColumn<string>(
                name: "AuthorType",
                table: "Publication",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
