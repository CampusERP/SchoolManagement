using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class ReconcileApplicationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_SchoolId_CampusId_Name",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_ClassRooms_SchoolId_CampusId_GradeLevelId_AcademicYearId_Name",
                table: "ClassRooms");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_SchoolId_CampusId_Name",
                table: "Rooms",
                columns: new[] { "SchoolId", "CampusId", "Name" },
                unique: true,
                filter: "[CampusId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClassRooms_SchoolId_CampusId_GradeLevelId_AcademicYearId_Name",
                table: "ClassRooms",
                columns: new[] { "SchoolId", "CampusId", "GradeLevelId", "AcademicYearId", "Name" },
                unique: true,
                filter: "[CampusId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_SchoolId_CampusId_Name",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_ClassRooms_SchoolId_CampusId_GradeLevelId_AcademicYearId_Name",
                table: "ClassRooms");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_SchoolId_CampusId_Name",
                table: "Rooms",
                columns: new[] { "SchoolId", "CampusId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassRooms_SchoolId_CampusId_GradeLevelId_AcademicYearId_Name",
                table: "ClassRooms",
                columns: new[] { "SchoolId", "CampusId", "GradeLevelId", "AcademicYearId", "Name" },
                unique: true);
        }
    }
}
