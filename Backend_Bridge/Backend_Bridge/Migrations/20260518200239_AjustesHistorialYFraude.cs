using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Bridge.Migrations
{
    /// <inheritdoc />
    public partial class AjustesHistorialYFraude : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "FraudAttempts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "FraudAttempts");
        }
    }
}
