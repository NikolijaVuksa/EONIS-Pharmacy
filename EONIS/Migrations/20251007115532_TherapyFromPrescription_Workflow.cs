using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EONIS.Migrations
{
    /// <inheritdoc />
    public partial class TherapyFromPrescription_Workflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Therapies");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Therapies",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                table: "Therapies",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DosageUnit",
                table: "Therapies",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DurationDays",
                table: "Therapies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PrescriptionCode",
                table: "Therapies",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Therapies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Therapies",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ReservationId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ReservationId",
                table: "Orders",
                column: "ReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Reservations_ReservationId",
                table: "Orders",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Reservations_ReservationId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ReservationId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DoctorName",
                table: "Therapies");

            migrationBuilder.DropColumn(
                name: "DosageUnit",
                table: "Therapies");

            migrationBuilder.DropColumn(
                name: "DurationDays",
                table: "Therapies");

            migrationBuilder.DropColumn(
                name: "PrescriptionCode",
                table: "Therapies");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Therapies");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Therapies");

            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Therapies",
                newName: "StartDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Therapies",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
