using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.ApplicationDb;

public partial class AddProfileDocumentsAndStudentFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "SecondName", table: "Students", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "ThirdName", table: "Students", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "NationalityCountry", table: "Students", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "ResidenceCountry", table: "Students", type: "nvarchar(100)", maxLength: 100, nullable: true);

        migrationBuilder.CreateTable(
            name: "DocumentAttachments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OwnerType = table.Column<int>(type: "int", nullable: false),
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
                table.PrimaryKey("PK_DocumentAttachments", x => x.Id);
                table.ForeignKey("FK_DocumentAttachments_Documents_DocumentId", x => x.DocumentId, "Documents", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_DocumentAttachments_DocumentId", table: "DocumentAttachments", column: "DocumentId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_DocumentAttachments_SchoolId_OwnerType_OwnerId", table: "DocumentAttachments", columns: new[] { "SchoolId", "OwnerType", "OwnerId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "DocumentAttachments");
        migrationBuilder.DropColumn(name: "SecondName", table: "Students");
        migrationBuilder.DropColumn(name: "ThirdName", table: "Students");
        migrationBuilder.DropColumn(name: "NationalityCountry", table: "Students");
        migrationBuilder.DropColumn(name: "ResidenceCountry", table: "Students");
    }
}
