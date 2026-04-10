using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanRequestInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixChecklistUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationDocumentChecklists_LoanApplicationId",
                table: "ApplicationDocumentChecklists");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocumentChecklists_LoanApplicationId",
                table: "ApplicationDocumentChecklists",
                column: "LoanApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationDocumentChecklists_LoanApplicationId",
                table: "ApplicationDocumentChecklists");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocumentChecklists_LoanApplicationId",
                table: "ApplicationDocumentChecklists",
                column: "LoanApplicationId",
                unique: true);
        }
    }
}
