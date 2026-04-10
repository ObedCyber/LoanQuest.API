using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanRequestInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Applicant_ApplicantEmployment_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applicant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MaritalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StateOfOrigin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResidentialAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ResidentialState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResidentialLGA = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BVN = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    NIN = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    KycStatus = table.Column<int>(type: "int", nullable: false),
                    KycVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProfileCompleteness = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applicant_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantEmployments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmploymentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmployerName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmployerAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmployerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    EmploymentStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    MonthlyGrossSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MonthlyNetSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SalaryAccountBank = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SalaryAccountNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsCurrentEmployer = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantEmployments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicantEmployments_Applicant_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applicant_BVN",
                table: "Applicant",
                column: "BVN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applicant_NIN",
                table: "Applicant",
                column: "NIN",
                unique: true,
                filter: "[NIN] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Applicant_UserId",
                table: "Applicant",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantEmployments_ApplicantId",
                table: "ApplicantEmployments",
                column: "ApplicantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicantEmployments");

            migrationBuilder.DropTable(
                name: "Applicant");
        }
    }
}
