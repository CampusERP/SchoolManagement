using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.ApplicationDb;

public partial class AddSchoolOwnedSubjects : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "SchoolId",
            table: "Subjects",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.DropIndex(
            name: "IX_Subjects_Code",
            table: "Subjects");

        migrationBuilder.CreateIndex(
            name: "IX_Subjects_Code",
            table: "Subjects",
            column: "Code",
            unique: true,
            filter: "[SchoolId] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Subjects_SchoolId_Code",
            table: "Subjects",
            columns: new[] { "SchoolId", "Code" },
            unique: true,
            filter: "[SchoolId] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Subjects_Code", table: "Subjects");
        migrationBuilder.DropIndex(name: "IX_Subjects_SchoolId_Code", table: "Subjects");
        migrationBuilder.DropColumn(name: "SchoolId", table: "Subjects");

        migrationBuilder.CreateIndex(
            name: "IX_Subjects_Code",
            table: "Subjects",
            column: "Code",
            unique: true);
    }
}
