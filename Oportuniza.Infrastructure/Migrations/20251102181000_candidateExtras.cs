using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Oportuniza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class candidateExtras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("d9e4c6a0-43e0-4568-bafc-265b4b47621b"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("e28fc01b-c6e6-4a4a-9e25-131d1f7b9e96"));

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.InsertData(
                table: "CompanyRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("d9e4c6a0-43e0-4568-bafc-265b4b47621b"), "Worker" },
                    { new Guid("e28fc01b-c6e6-4a4a-9e25-131d1f7b9e96"), "Administrator" }
                });
        }
    }
}
