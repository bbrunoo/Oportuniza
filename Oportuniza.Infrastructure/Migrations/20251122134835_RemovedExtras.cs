using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Oportuniza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedExtras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateApplications_Publication_PublicationId1",
                table: "CandidateApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_CandidateApplications_User_UserId1",
                table: "CandidateApplications");

            migrationBuilder.DropIndex(
                name: "IX_CandidateApplications_PublicationId1",
                table: "CandidateApplications");

            migrationBuilder.DropIndex(
                name: "IX_CandidateApplications_UserId1",
                table: "CandidateApplications");

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("14282161-01c3-4ae9-aa11-1447b90d8663"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("513c143b-d165-4acb-b440-7bea03bdabb3"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("c63e1e73-900f-4022-92d9-9c83ceef0584"));

            migrationBuilder.DropColumn(
                name: "PublicationId1",
                table: "CandidateApplications");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "CandidateApplications");

            migrationBuilder.InsertData(
                table: "CompanyRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("17aa293c-fe42-42c9-9e3f-6491f4c55dec"), "Owner" },
                    { new Guid("35ac69e3-bc79-4ac1-85f9-72a77f4484a0"), "Administrator" },
                    { new Guid("67104519-348a-4a18-a020-c17bf8556e79"), "Worker" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("17aa293c-fe42-42c9-9e3f-6491f4c55dec"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("35ac69e3-bc79-4ac1-85f9-72a77f4484a0"));

            migrationBuilder.DeleteData(
                table: "CompanyRole",
                keyColumn: "Id",
                keyValue: new Guid("67104519-348a-4a18-a020-c17bf8556e79"));

            migrationBuilder.AddColumn<Guid>(
                name: "PublicationId1",
                table: "CandidateApplications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "CandidateApplications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.InsertData(
                table: "CompanyRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("14282161-01c3-4ae9-aa11-1447b90d8663"), "Owner" },
                    { new Guid("513c143b-d165-4acb-b440-7bea03bdabb3"), "Administrator" },
                    { new Guid("c63e1e73-900f-4022-92d9-9c83ceef0584"), "Worker" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateApplications_PublicationId1",
                table: "CandidateApplications",
                column: "PublicationId1");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateApplications_UserId1",
                table: "CandidateApplications",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateApplications_Publication_PublicationId1",
                table: "CandidateApplications",
                column: "PublicationId1",
                principalTable: "Publication",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateApplications_User_UserId1",
                table: "CandidateApplications",
                column: "UserId1",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
