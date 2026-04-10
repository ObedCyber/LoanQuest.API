using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanRequestInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EligibilityChecks_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EligibilityChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequestedTenorMonths = table.Column<int>(type: "int", nullable: false),
                    MonthlyGrossSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyObligations = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DisposableIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxEligibleAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinEligibleAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecommendedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxMonthlyRepayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EffectiveInterestRate = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    DSRApplied = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsEligible = table.Column<bool>(type: "bit", nullable: false),
                    RejectionReasons = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskRating = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibilityChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EligibilityChecks_Applicant_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EligibilityChecks_LoanProducts_LoanProductId",
                        column: x => x.LoanProductId,
                        principalTable: "LoanProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityChecks_ApplicantId",
                table: "EligibilityChecks",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityChecks_ExpiresAt",
                table: "EligibilityChecks",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityChecks_LoanProductId",
                table: "EligibilityChecks",
                column: "LoanProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EligibilityChecks");
        }
    }
}
