using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservePoint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IdentityId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Policy_AllowedTimeFrom",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Policy_AllowedTimeTo",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Policy_MaxBookingsPerUser",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Policy_MaxDurationHours",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Bookings");

            migrationBuilder.RenameTable(
                name: "Bookings",
                newName: "Booking");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Booking",
                newName: "BookingGroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Booking",
                table: "Booking",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BookingGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentityId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Policy_MaxDurationHours = table.Column<int>(type: "integer", nullable: false),
                    Policy_MaxBookingsPerUser = table.Column<int>(type: "integer", nullable: false),
                    Policy_AllowedTimeFrom = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Policy_AllowedTimeTo = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_BookingGroupId",
                table: "Booking",
                column: "BookingGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_BookingGroups_BookingGroupId",
                table: "Booking",
                column: "BookingGroupId",
                principalTable: "BookingGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_BookingGroups_BookingGroupId",
                table: "Booking");

            migrationBuilder.DropTable(
                name: "BookingGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Booking",
                table: "Booking");

            migrationBuilder.DropIndex(
                name: "IX_Booking_BookingGroupId",
                table: "Booking");

            migrationBuilder.RenameTable(
                name: "Booking",
                newName: "Bookings");

            migrationBuilder.RenameColumn(
                name: "BookingGroupId",
                table: "Bookings",
                newName: "OrganizationId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IdentityId",
                table: "Bookings",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Policy_AllowedTimeFrom",
                table: "Bookings",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Policy_AllowedTimeTo",
                table: "Bookings",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "Policy_MaxBookingsPerUser",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Policy_MaxDurationHours",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings",
                column: "Id");
        }
    }
}
