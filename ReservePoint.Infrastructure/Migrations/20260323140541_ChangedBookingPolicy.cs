using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservePoint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedBookingPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Policy_AllowedTimeFrom",
                table: "BookingGroups");

            migrationBuilder.DropColumn(
                name: "Policy_AllowedTimeTo",
                table: "BookingGroups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "Policy_AllowedTimeFrom",
                table: "BookingGroups",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Policy_AllowedTimeTo",
                table: "BookingGroups",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
