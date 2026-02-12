using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeesMVC_Core_8.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeelocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Employees",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Employees",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Employees");
        }
    }
}
