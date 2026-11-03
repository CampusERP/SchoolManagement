using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.ApplicationDb;

public partial class AddCampusAssignments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(name: "CampusId", table: "Students", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "CampusId", table: "Rooms", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "CampusId", table: "ClassRooms", type: "uniqueidentifier", nullable: true);

        migrationBuilder.DropIndex(name: "IX_Rooms_SchoolId_Name", table: "Rooms");
        migrationBuilder.DropIndex(name: "IX_ClassRooms_SchoolId_GradeLevelId_AcademicYearId_Name", table: "ClassRooms");
        migrationBuilder.CreateIndex(name: "IX_Rooms_SchoolId_CampusId_Name", table: "Rooms", columns: new[] { "SchoolId", "CampusId", "Name" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_ClassRooms_SchoolId_CampusId_GradeLevelId_AcademicYearId_Name", table: "ClassRooms", columns: new[] { "SchoolId", "CampusId", "GradeLevelId", "AcademicYearId", "Name" }, unique: true);

        migrationBuilder.CreateTable(
            name: "TeacherCampuses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CampusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TeacherCampuses", x => x.Id);
                table.ForeignKey("FK_TeacherCampuses_Teachers_TeacherId", x => x.TeacherId, "Teachers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_Students_SchoolId_CampusId", "Students", new[] { "SchoolId", "CampusId" });
        migrationBuilder.CreateIndex(
            name: "IX_TeacherCampuses_SchoolId_TeacherId_CampusId",
            table: "TeacherCampuses",
            columns: new[] { "SchoolId", "TeacherId", "CampusId" },
            unique: true,
            filter: "[IsDeleted] = 0");
        migrationBuilder.CreateIndex("IX_TeacherCampuses_TeacherId", "TeacherCampuses", "TeacherId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("TeacherCampuses");
        migrationBuilder.DropIndex("IX_Students_SchoolId_CampusId", "Students");
        migrationBuilder.DropIndex("IX_Rooms_SchoolId_CampusId_Name", "Rooms");
        migrationBuilder.DropIndex("IX_ClassRooms_SchoolId_CampusId_GradeLevelId_AcademicYearId_Name", "ClassRooms");
        migrationBuilder.DropColumn("CampusId", "Students");
        migrationBuilder.DropColumn("CampusId", "Rooms");
        migrationBuilder.DropColumn("CampusId", "ClassRooms");
        migrationBuilder.CreateIndex(name: "IX_Rooms_SchoolId_Name", table: "Rooms", columns: new[] { "SchoolId", "Name" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_ClassRooms_SchoolId_GradeLevelId_AcademicYearId_Name", table: "ClassRooms", columns: new[] { "SchoolId", "GradeLevelId", "AcademicYearId", "Name" }, unique: true);
    }
}
