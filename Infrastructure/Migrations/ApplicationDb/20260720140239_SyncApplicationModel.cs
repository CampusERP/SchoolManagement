using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class SyncApplicationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "ExamResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "AttendanceRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "AssignmentSubmissions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_SchoolId",
                table: "ExamResults",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_SchoolId",
                table: "AttendanceRecords",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_SchoolId",
                table: "AssignmentSubmissions",
                column: "SchoolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExamResults_SchoolId",
                table: "ExamResults");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_SchoolId",
                table: "AttendanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_AssignmentSubmissions_SchoolId",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "AssignmentSubmissions");
        }
    }
}
