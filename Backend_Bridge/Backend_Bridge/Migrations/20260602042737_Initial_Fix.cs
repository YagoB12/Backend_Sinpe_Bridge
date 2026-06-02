using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Bridge.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RiskId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Risks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Risks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_RiskId",
                table: "Payments",
                column: "RiskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Risks_RiskId",
                table: "Payments",
                column: "RiskId",
                principalTable: "Risks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Risks_RiskId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "Risks");

            migrationBuilder.DropIndex(
                name: "IX_Payments_RiskId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RiskId",
                table: "Payments");
        }
    }
}
