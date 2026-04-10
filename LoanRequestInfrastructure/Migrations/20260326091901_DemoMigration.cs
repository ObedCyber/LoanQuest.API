using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanRequestInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DemoMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicantEmployments_ApplicantId",
                table: "ApplicantEmployments");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantEmployments_ApplicantId",
                table: "ApplicantEmployments",
                column: "ApplicantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicantEmployments_ApplicantId",
                table: "ApplicantEmployments");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantEmployments_ApplicantId",
                table: "ApplicantEmployments",
                column: "ApplicantId");
        }
    }
}
