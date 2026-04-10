using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LoanRequestInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Document_Requirements_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    MaxFileSizeMb = table.Column<int>(type: "int", nullable: false),
                    AllowedFileTypes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentRequirements_LoanProducts_LoanProductId",
                        column: x => x.LoanProductId,
                        principalTable: "LoanProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DocumentRequirements",
                columns: new[] { "Id", "AllowedFileTypes", "Description", "DocumentTypeCode", "DocumentTypeName", "IsMandatory", "LoanProductId", "MaxFileSizeMb" },
                values: new object[,]
                {
                    { new Guid("11948273-1111-0000-0000-000000000001"), "pdf,jpg,png", "Provide payslips for the last 3 consecutive months showing employer name and net salary.", "PAYSLIP", "Last 3 Months Payslip", true, new Guid("01948273-1111-4444-8888-000000000001"), 5 },
                    { new Guid("11948273-1111-0000-0000-000000000002"), "pdf", "Official bank statement showing salary entries for the last 6 months.", "BANK_STMT", "6 Months Bank Statement", true, new Guid("01948273-1111-4444-8888-000000000001"), 10 },
                    { new Guid("11948273-1111-0000-0000-000000000003"), "pdf,jpg", "Letter from your employer confirming your job status and length of service.", "EMP_LETTER", "Employment Confirmation Letter", true, new Guid("01948273-1111-4444-8888-000000000001"), 2 },
                    { new Guid("11948273-1111-0000-0000-000000000004"), "pdf,jpg,png", "Valid Passport, Driver's License, or National ID Card.", "VALID_ID", "Government Issued ID", true, new Guid("01948273-1111-4444-8888-000000000001"), 2 },
                    { new Guid("11948273-1111-0000-0000-000000000005"), "pdf,jpg,png", "Recent utility bill (Electricity, Water, or Waste) not older than 3 months.", "UTILITY_BILL", "Proof of Address", false, new Guid("01948273-1111-4444-8888-000000000001"), 2 },
                    { new Guid("11948273-2222-0000-0000-000000000001"), "jpg,png", "A clear photo of your current staff ID card.", "COMPANY_ID", "Staff Identity Card", true, new Guid("01948273-2222-4444-8888-000000000002"), 2 },
                    { new Guid("11948273-2222-0000-0000-000000000002"), "pdf,jpg", "Last month's payslip for salary validation.", "PAYSLIP", "Most Recent Payslip", true, new Guid("01948273-2222-4444-8888-000000000002"), 2 },
                    { new Guid("11948273-3333-0000-0000-000000000001"), "pdf", "Official Certificate of Incorporation from the Corporate Affairs Commission.", "CAC_CERT", "CAC Certificate", true, new Guid("01948273-3333-4444-8888-000000000003"), 5 },
                    { new Guid("11948273-3333-0000-0000-000000000002"), "pdf", "Corporate bank statement for the last 12 months.", "BANK_STMT", "12 Months Bank Statement", true, new Guid("01948273-3333-4444-8888-000000000003"), 15 },
                    { new Guid("11948273-3333-0000-0000-000000000003"), "pdf", "Last 2 years audited financial statements signed by a certified accountant.", "AUDITED_ACCTS", "Audited Accounts", true, new Guid("01948273-3333-4444-8888-000000000003"), 10 },
                    { new Guid("11948273-3333-0000-0000-000000000004"), "pdf,jpg,png", "Valid government ID of at least one director.", "VALID_ID", "Director's ID", true, new Guid("01948273-3333-4444-8888-000000000003"), 2 },
                    { new Guid("11948273-4444-0000-0000-000000000001"), "pdf,jpg,png", "Passport, Driver's License or National ID for immediate verification.", "VALID_ID", "Government Issued ID", true, new Guid("01948273-4444-4444-8888-000000000004"), 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRequirements_LoanProductId",
                table: "DocumentRequirements",
                column: "LoanProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentRequirements");
        }
    }
}
