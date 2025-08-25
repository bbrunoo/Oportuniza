using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oportuniza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class loginchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certification_Curriculum_CurriculumId",
                table: "Certification");

            migrationBuilder.DropForeignKey(
                name: "FK_Education_Curriculum_CurriculumId",
                table: "Education");

            migrationBuilder.DropForeignKey(
                name: "FK_Experience_Curriculum_CurriculumId",
                table: "Experience");

            migrationBuilder.DropTable(
                name: "Curriculum");

            migrationBuilder.DropIndex(
                name: "IX_Experience_CurriculumId",
                table: "Experience");

            migrationBuilder.DropIndex(
                name: "IX_Education_CurriculumId",
                table: "Education");

            migrationBuilder.DropIndex(
                name: "IX_Certification_CurriculumId",
                table: "Certification");

            migrationBuilder.DropColumn(
                name: "CurriculumId",
                table: "Experience");

            migrationBuilder.DropColumn(
                name: "CurriculumId",
                table: "Education");

            migrationBuilder.CreateTable(
                name: "UserLogin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentityProvider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLogin_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLogin_UserId",
                table: "UserLogin",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLogin");

            migrationBuilder.AddColumn<Guid>(
                name: "CurriculumId",
                table: "Experience",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CurriculumId",
                table: "Education",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Curriculum",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Objective = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Curriculum", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Curriculum_City_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Curriculum_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Experience_CurriculumId",
                table: "Experience",
                column: "CurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_Education_CurriculumId",
                table: "Education",
                column: "CurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_Certification_CurriculumId",
                table: "Certification",
                column: "CurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_Curriculum_CityId",
                table: "Curriculum",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Curriculum_UserId",
                table: "Curriculum",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Certification_Curriculum_CurriculumId",
                table: "Certification",
                column: "CurriculumId",
                principalTable: "Curriculum",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Education_Curriculum_CurriculumId",
                table: "Education",
                column: "CurriculumId",
                principalTable: "Curriculum",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Experience_Curriculum_CurriculumId",
                table: "Experience",
                column: "CurriculumId",
                principalTable: "Curriculum",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
