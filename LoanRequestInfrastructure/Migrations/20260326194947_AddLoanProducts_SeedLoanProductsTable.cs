using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LoanRequestInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanProducts_SeedLoanProductsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoanProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    LoanType = table.Column<int>(type: "int", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinTenorMonths = table.Column<int>(type: "int", nullable: false),
                    MaxTenorMonths = table.Column<int>(type: "int", nullable: false),
                    InterestRatePercent = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    InterestRateType = table.Column<int>(type: "int", nullable: false),
                    MaxLTIMultiplier = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MaxDSRPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RequiredDocumentTypes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EligibilityCriteria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanProducts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "LoanProducts",
                columns: new[] { "Id", "CreatedAt", "EligibilityCriteria", "InterestRatePercent", "InterestRateType", "IsActive", "LoanType", "MaxAmount", "MaxDSRPercent", "MaxLTIMultiplier", "MaxTenorMonths", "MinAmount", "MinTenorMonths", "Name", "ProductCode", "RequiredDocumentTypes", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("01948273-1111-4444-8888-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 18.00m, 0, true, 0, 50000000m, 33.00m, 60.00m, 60, 50000m, 6, "Personal Loan", "PLN001", "[\"GovID\", \"UtilityBill\", \"3MonthsPayslip\"]", null },
                    { new Guid("01948273-2222-4444-8888-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5.00m, 2, true, 1, 1000000m, 40.00m, 3.00m, 12, 10000m, 1, "Salary Advance", "SADV001", "[\"CompanyID\", \"1MonthPayslip\"]", null },
                    { new Guid("01948273-3333-4444-8888-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 22.00m, 1, true, 2, 100000000m, 35.00m, 0.00m, 84, 500000m, 12, "Business Loan", "BIZ001", "[\"CAC_Docs\", \"6MonthsBankStatement\"]", null },
                    { new Guid("01948273-4444-4444-8888-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 10.00m, 2, true, 5, 100000m, 50.00m, 1.00m, 6, 5000m, 1, "Emergency Loan", "EMG001", "[\"GovID\"]", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoanProducts");
        }
    }
}
