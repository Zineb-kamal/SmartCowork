using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCowork.Services.Booking.Migrations
{
    /// <inheritdoc />
    public partial class AddPurposeAndAttendees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttendeesCount",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendeesCount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "Bookings");
        }
    }
}
