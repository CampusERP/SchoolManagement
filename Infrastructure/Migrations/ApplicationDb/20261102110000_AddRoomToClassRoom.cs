using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.ApplicationDb;

public partial class AddRoomToClassRoom : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "RoomId",
            table: "ClassRooms",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_ClassRooms_RoomId",
            table: "ClassRooms",
            column: "RoomId");

        migrationBuilder.AddForeignKey(
            name: "FK_ClassRooms_Rooms_RoomId",
            table: "ClassRooms",
            column: "RoomId",
            principalTable: "Rooms",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ClassRooms_Rooms_RoomId",
            table: "ClassRooms");

        migrationBuilder.DropIndex(
            name: "IX_ClassRooms_RoomId",
            table: "ClassRooms");

        migrationBuilder.DropColumn(
            name: "RoomId",
            table: "ClassRooms");
    }
}
