using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Oportuniza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class extrachange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateExtra_CandidateApplications_CandidateApplicationId1",
                table: "CandidateExtra");

            migrationBuilder.DropIndex(
                name: "IX_CandidateExtra_CandidateApplicationId1",
                table: "CandidateExtra");

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("25a08e31-50d4-41bc-bb17-dfbfa49b0195"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("4dada3ed-d4f2-46ac-8b07-5760c546414e"));

            migrationBuilder.DropColumn(
                name: "CandidateApplicationId1",
                table: "CandidateExtra");

            migrationBuilder.AlterColumn<string>(
                name: "ResumeUrl",
                table: "CandidateExtra",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldUnicode: false,
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observation",
                table: "CandidateExtra",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "CompanyRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("c39fffd0-c14c-40fa-8c28-a106f10932d5"), "Worker" },
                    { new Guid("e91625a8-c6ed-434e-bd1f-3d6c251a3b74"), "Administrator" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("c39fffd0-c14c-40fa-8c28-a106f10932d5"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("e91625a8-c6ed-434e-bd1f-3d6c251a3b74"));

            migrationBuilder.AlterColumn<string>(
                name: "ResumeUrl",
                table: "CandidateExtra",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observation",
                table: "CandidateExtra",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CandidateApplicationId1",
                table: "CandidateExtra",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.InsertData(
                table: "CompanyRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("25a08e31-50d4-41bc-bb17-dfbfa49b0195"), "Administrator" },
                    { new Guid("4dada3ed-d4f2-46ac-8b07-5760c546414e"), "Worker" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateExtra_CandidateApplicationId1",
                table: "CandidateExtra",
                column: "CandidateApplicationId1",
                unique: true,
                filter: "[CandidateApplicationId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateExtra_CandidateApplications_CandidateApplicationId1",
                table: "CandidateExtra",
                column: "CandidateApplicationId1",
                principalTable: "CandidateApplications",
                principalColumn: "Id");
        }
    }
}
