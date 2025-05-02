using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCowork.Services.Space.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlAndCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Spaces",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Spaces");
        }
    }
}
