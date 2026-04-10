using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanRequestInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Applicant_Navigations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplication_Applicant_ApplicantId",
                table: "LoanApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplication_EligibilityChecks_EligibilityCheckId",
                table: "LoanApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplication_LoanProducts_LoanProductId",
                table: "LoanApplication");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoanApplication",
                table: "LoanApplication");

            migrationBuilder.DropIndex(
                name: "IX_LoanApplication_EligibilityCheckId",
                table: "LoanApplication");

            migrationBuilder.RenameTable(
                name: "LoanApplication",
                newName: "LoanApplications");

            migrationBuilder.RenameIndex(
                name: "IX_LoanApplication_LoanProductId",
                table: "LoanApplications",
                newName: "IX_LoanApplications_LoanProductId");

            migrationBuilder.RenameIndex(
                name: "IX_LoanApplication_ApplicationNumber",
                table: "LoanApplications",
                newName: "IX_LoanApplications_ApplicationNumber");

            migrationBuilder.RenameIndex(
                name: "IX_LoanApplication_ApplicantId",
                table: "LoanApplications",
                newName: "IX_LoanApplications_ApplicantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoanApplications",
                table: "LoanApplications",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_EligibilityCheckId",
                table: "LoanApplications",
                column: "EligibilityCheckId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplications_Applicant_ApplicantId",
                table: "LoanApplications",
                column: "ApplicantId",
                principalTable: "Applicant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplications_EligibilityChecks_EligibilityCheckId",
                table: "LoanApplications",
                column: "EligibilityCheckId",
                principalTable: "EligibilityChecks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplications_LoanProducts_LoanProductId",
                table: "LoanApplications",
                column: "LoanProductId",
                principalTable: "LoanProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplications_Applicant_ApplicantId",
                table: "LoanApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplications_EligibilityChecks_EligibilityCheckId",
                table: "LoanApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplications_LoanProducts_LoanProductId",
                table: "LoanApplications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoanApplications",
                table: "LoanApplications");

            migrationBuilder.DropIndex(
                name: "IX_LoanApplications_EligibilityCheckId",
                table: "LoanApplications");

            migrationBuilder.RenameTable(
                name: "LoanApplications",
                newName: "LoanApplication");

            migrationBuilder.RenameIndex(
                name: "IX_LoanApplications_LoanProductId",
                table: "LoanApplication",
                newName: "IX_LoanApplication_LoanProductId");

            migrationBuilder.RenameIndex(
                name: "IX_LoanApplications_ApplicationNumber",
                table: "LoanApplication",
                newName: "IX_LoanApplication_ApplicationNumber");

            migrationBuilder.RenameIndex(
                name: "IX_LoanApplications_ApplicantId",
                table: "LoanApplication",
                newName: "IX_LoanApplication_ApplicantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoanApplication",
                table: "LoanApplication",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_EligibilityCheckId",
                table: "LoanApplication",
                column: "EligibilityCheckId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplication_Applicant_ApplicantId",
                table: "LoanApplication",
                column: "ApplicantId",
                principalTable: "Applicant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplication_EligibilityChecks_EligibilityCheckId",
                table: "LoanApplication",
                column: "EligibilityCheckId",
                principalTable: "EligibilityChecks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplication_LoanProducts_LoanProductId",
                table: "LoanApplication",
                column: "LoanProductId",
                principalTable: "LoanProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
