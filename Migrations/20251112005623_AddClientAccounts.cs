using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Table_Reservation.Migrations
{
    /// <inheritdoc />
    public partial class AddClientAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "Reservations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ClientModels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "ClientModels",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ClientModels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ClientId",
                table: "Reservations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientModels_ClientEmail",
                table: "ClientModels",
                column: "ClientEmail",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ClientModels_ClientId",
                table: "Reservations",
                column: "ClientId",
                principalTable: "ClientModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ClientModels_ClientId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ClientId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_ClientModels_ClientEmail",
                table: "ClientModels");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ClientModels");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "ClientModels");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ClientModels");
        }
    }
}
