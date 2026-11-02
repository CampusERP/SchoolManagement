using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.Platform
{
    /// <inheritdoc />
    public partial class MakePaymentRowVersionConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL Server cannot ALTER a varbinary column into rowversion, so the
            // column is recreated. Payments could never be inserted before this
            // fix (RowVersion was required but never populated), so no data is lost.
            migrationBuilder.DropColumn(name: "RowVersion", table: "Payments");
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Payments",
                type: "rowversion",
                rowVersion: true,
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "RowVersion", table: "Payments");
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Payments",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: Array.Empty<byte>());
        }
    }
}
