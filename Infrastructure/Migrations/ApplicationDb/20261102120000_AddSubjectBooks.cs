using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.ApplicationDb;

public partial class AddSubjectBooks : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_CurriculumSubjects_SchoolId_GradeLevelId_SubjectId",
            table: "CurriculumSubjects");

        migrationBuilder.CreateIndex(
            name: "IX_CurriculumSubjects_SchoolId_GradeLevelId_SubjectId",
            table: "CurriculumSubjects",
            columns: new[] { "SchoolId", "GradeLevelId", "SubjectId" },
            unique: true,
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateTable(
            name: "SubjectBooks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CurriculumSubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Author = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                Publisher = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                Isbn = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                Edition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubjectBooks", x => x.Id);
                table.ForeignKey(
                    name: "FK_SubjectBooks_CurriculumSubjects_CurriculumSubjectId",
                    column: x => x.CurriculumSubjectId,
                    principalTable: "CurriculumSubjects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SubjectBooks_CurriculumSubjectId",
            table: "SubjectBooks",
            column: "CurriculumSubjectId");
        migrationBuilder.CreateIndex(
            name: "IX_SubjectBooks_SchoolId",
            table: "SubjectBooks",
            column: "SchoolId");
        migrationBuilder.CreateIndex(
            name: "IX_SubjectBooks_SchoolId_CurriculumSubjectId",
            table: "SubjectBooks",
            columns: new[] { "SchoolId", "CurriculumSubjectId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SubjectBooks");
        migrationBuilder.DropIndex(name: "IX_CurriculumSubjects_SchoolId_GradeLevelId_SubjectId", table: "CurriculumSubjects");
        migrationBuilder.CreateIndex(
            name: "IX_CurriculumSubjects_SchoolId_GradeLevelId_SubjectId",
            table: "CurriculumSubjects",
            columns: new[] { "SchoolId", "GradeLevelId", "SubjectId" },
            unique: true);
    }
}
