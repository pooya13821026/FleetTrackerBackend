using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDriver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationalCode",
                table: "Drivers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NationalCode",
                table: "Drivers");
        }
    }
}
