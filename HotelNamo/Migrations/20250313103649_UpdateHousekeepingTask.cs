using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelNamo.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHousekeepingTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "HousekeepingTasks");

            migrationBuilder.RenameColumn(
                name: "ScheduledTime",
                table: "HousekeepingTasks",
                newName: "DueDate");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "HousekeepingTasks",
                newName: "TaskDescription");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "HousekeepingTasks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AssignedStaffId",
                table: "HousekeepingTasks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "HousekeepingTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "HousekeepingTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_HousekeepingTasks_AssignedStaffId",
                table: "HousekeepingTasks",
                column: "AssignedStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_HousekeepingTasks_AspNetUsers_AssignedStaffId",
                table: "HousekeepingTasks",
                column: "AssignedStaffId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HousekeepingTasks_AspNetUsers_AssignedStaffId",
                table: "HousekeepingTasks");

            migrationBuilder.DropIndex(
                name: "IX_HousekeepingTasks_AssignedStaffId",
                table: "HousekeepingTasks");

            migrationBuilder.DropColumn(
                name: "AssignedStaffId",
                table: "HousekeepingTasks");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "HousekeepingTasks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "HousekeepingTasks");

            migrationBuilder.RenameColumn(
                name: "TaskDescription",
                table: "HousekeepingTasks",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "HousekeepingTasks",
                newName: "ScheduledTime");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "HousekeepingTasks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "HousekeepingTasks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
