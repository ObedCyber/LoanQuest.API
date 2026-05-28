using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using LoanRequestDomain.Entities;
using LoanRequestDomain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static LoanRequestDomain.Entities.EligibilityChecks;

namespace LoanRequestInfrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options) { }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Applicant> Applicant { get; set; }
        public DbSet<ApplicantEmployment> ApplicantEmployments { get; set; }
        public DbSet<ApplicantFinancials> ApplicantFinancials { get; set; }
        public DbSet<EligibilityChecks> EligibilityChecks { get; set; }
        public DbSet<LoanProducts> LoanProducts { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<LoanDocument> LoanDocuments { get; set; }

        public DbSet<DocumentRequirement> DocumentRequirements { get; set; }
        public DbSet<ApplicationDocumentChecklist> ApplicationDocumentChecklists { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Ensure Token is unique and indexed 
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.Property(e => e.DeviceInfo)
                    .HasMaxLength(256);
                entity.HasOne<IdentityUser>()
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .IsRequired()
              .OnDelete(DeleteBehavior.Cascade);

            });

            builder.Entity<Applicant>(entity => {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
        
                entity.HasOne(a => a.User)           
                      .WithOne()                     
                      .HasForeignKey<Applicant>(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.HasIndex(e => e.UserId).IsUnique(); 
                entity.HasIndex(e => e.BVN).IsUnique();   
                entity.HasIndex(e => e.NIN).IsUnique();

                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);

                entity.Property(e => e.DateOfBirth).HasColumnType("date");
            });

            builder.Entity<ApplicantEmployment>(entity => {
                entity.HasOne(a => a.Applicant)
                .WithOne(a => a.Employment)
                .HasForeignKey<ApplicantEmployment>(a => a.ApplicantId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ApplicantId).IsUnique(); 
                
            });

            builder.Entity<ApplicantFinancials>(entity => {
                entity.HasOne(a => a.Applicant)
                .WithOne(a => a.Financials)
                .HasForeignKey<ApplicantFinancials>(a => a.ApplicantId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ApplicantId).IsUnique();
            });

            builder.Entity<LoanApplication>(entity =>
            {
                entity.HasIndex(e => e.ApplicationNumber).IsUnique();
                entity.HasIndex(e => e.EligibilityCheckId).IsUnique();

                entity.Property(e => e.RequestedAmount).HasPrecision(18, 2);
                entity.Property(e => e.ApprovedAmount).HasPrecision(18, 2);
                entity.Property(e => e.InterestRate).HasPrecision(8, 4);
                entity.Property(e => e.MonthlyRepayment).HasPrecision(18, 2);
                entity.Property(e => e.TotalRepayable).HasPrecision(18, 2);

                entity.HasQueryFilter(e => !e.IsDeleted);
                
                entity.HasOne(d => d.Applicant)
                    .WithMany(a => a.LoanApplications)
                    .HasForeignKey(d => d.ApplicantId)
                    .OnDelete(DeleteBehavior.NoAction); // Prevents cascade delete cycles

                entity.HasOne(p => p.LoanProduct);
            });

            builder.Entity<LoanDocument>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.LoanApplicationId);
                entity.HasIndex(e => e.ApplicantId);

                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(d => d.LoanApplication)
                      .WithMany() 
                      .HasForeignKey(d => d.LoanApplicationId)
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.HasOne(d => d.Applicant)
                      .WithMany()
                      .HasForeignKey(d => d.ApplicantId)
                      .OnDelete(DeleteBehavior.NoAction); 
            });

            builder.Entity<DocumentRequirement>()
                .HasOne(d => d.LoanProduct)
                .WithMany(p => p.DocumentRequirements)
                .HasForeignKey(d => d.LoanProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EligibilityChecks>()
            .HasOne(e => e.Applicant)
            .WithMany(a => a.EligibilityChecks) 
            .HasForeignKey(e => e.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);
         
            builder.Entity<EligibilityChecks>()
                .HasOne(e => e.LoanProduct)
                .WithMany() 
                .HasForeignKey(e => e.LoanProductId)
                .OnDelete(DeleteBehavior.Restrict); // Don't delete checks if a product is deleted

            builder.Entity<ApplicationDocumentChecklist>()
              .HasOne(c => c.LoanApplication)
              .WithMany(a => a.DocumentChecklist)
              .HasForeignKey(c => c.LoanApplicationId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationDocumentChecklist>()
                .HasQueryFilter(e => !e.IsDeleted);

            builder.Entity<EligibilityChecks>()
            .HasIndex(e => e.ApplicantId);

            builder.Entity<EligibilityChecks>()
                .HasIndex(e => e.ExpiresAt);

            builder.Entity<DocumentRequirement>().HasData(
             // --- PERSONAL LOAN REQUIREMENTS (01948273-1111-...) ---
            new DocumentRequirement
            {
                Id = Guid.Parse("11948273-1111-0000-0000-000000000001"),
                LoanProductId = Guid.Parse("01948273-1111-4444-8888-000000000001"),
                DocumentTypeCode = "PAYSLIP",
                DocumentTypeName = "Last 3 Months Payslip",
                Description = "Provide payslips for the last 3 consecutive months showing employer name and net salary.",
                IsMandatory = true,
                MaxFileSizeMb = 5,
                AllowedFileTypes = "pdf,jpg,png"
            },
            new DocumentRequirement
            {
                Id = Guid.Parse("11948273-1111-0000-0000-000000000002"),
                LoanProductId = Guid.Parse("01948273-1111-4444-8888-000000000001"),
                DocumentTypeCode = "BANK_STMT",
                DocumentTypeName = "6 Months Bank Statement",
                Description = "Official bank statement showing salary entries for the last 6 months.",
                IsMandatory = true,
                MaxFileSizeMb = 10,
                AllowedFileTypes = "pdf"
            },
            new DocumentRequirement
            {
                Id = Guid.Parse("11948273-1111-0000-0000-000000000003"),
                LoanProductId = Guid.Parse("01948273-1111-4444-8888-000000000001"),
                DocumentTypeCode = "EMP_LETTER",
                DocumentTypeName = "Employment Confirmation Letter",
                Description = "Letter from your employer confirming your job status and length of service.",
                IsMandatory = true,
                MaxFileSizeMb = 2,
                AllowedFileTypes = "pdf,jpg"
            },
            new DocumentRequirement
            {
                Id = Guid.Parse("11948273-1111-0000-0000-000000000004"),
                LoanProductId = Guid.Parse("01948273-1111-4444-8888-000000000001"),
                DocumentTypeCode = "VALID_ID",
                DocumentTypeName = "Government Issued ID",
                Description = "Valid Passport, Driver's License, or National ID Card.",
                IsMandatory = true,
                MaxFileSizeMb = 2,
                AllowedFileTypes = "pdf,jpg,png"
            },
            new DocumentRequirement
            {
                Id = Guid.Parse("11948273-1111-0000-0000-000000000005"),
                LoanProductId = Guid.Parse("01948273-1111-4444-8888-000000000001"),
                DocumentTypeCode = "UTILITY_BILL",
                DocumentTypeName = "Proof of Address",
                Description = "Recent utility bill (Electricity, Water, or Waste) not older than 3 months.",
                IsMandatory = false,
                MaxFileSizeMb = 2,
                AllowedFileTypes = "pdf,jpg,png"
            },

            // --- SALARY ADVANCE REQUIREMENTS (01948273-2222-...) ---
            new DocumentRequirement
            {
                Id = Guid.Parse("11948273-2222-0000-0000-000000000001"),
                LoanProductId = Guid.Parse("01948273-2222-4444-8888-000000000002"),
                DocumentTypeCode = "COMPANY_ID",
                DocumentTypeName = "Staff Identity Card",
                Description = "A clear photo of your current staff ID card.",
                IsMandatory = true,
                MaxFileSizeMb = 2,
                AllowedFileTypes = "jpg,png"
            },
            new DocumentRequirement
            {
                Id = Guid.Parse("11948273-2222-0000-0000-000000000002"),
                LoanProductId = Guid.Parse("01948273-2222-4444-8888-000000000002"),
                DocumentTypeCode = "PAYSLIP",
                DocumentTypeName = "Most Recent Payslip",
                Description = "Last month's payslip for salary validation.",
                IsMandatory = true,
                MaxFileSizeMb = 2,
                AllowedFileTypes = "pdf,jpg"
            },

            new DocumentRequirement
            {
                Id = Guid.Parse("11948273-3333-0000-0000-000000000001"),
                LoanProductId = Guid.Parse("01948273-3333-4444-8888-000000000003"),
                DocumentTypeCode = "CAC_CERT",
                DocumentTypeName = "CAC Certificate",
                Description = "Official Certificate of Incorporation from the Corporate Affairs Commission.",
                IsMandatory = true,
                MaxFileSizeMb = 5,
                AllowedFileTypes = "pdf"
            },
            new DocumentRequirement
            {
                Id = Guid.Parse("11948273-3333-0000-0000-000000000002"),
                LoanProductId = Guid.Parse("01948273-3333-4444-8888-000000000003"),
                DocumentTypeCode = "BANK_STMT",
                DocumentTypeName = "12 Months Bank Statement",
                Description = "Corporate bank statement for the last 12 months.",
                IsMandatory = true,
                MaxFileSizeMb = 15,
                AllowedFileTypes = "pdf"
            },
            new DocumentRequirement
            {
                Id = Guid.Parse("11948273-3333-0000-0000-000000000003"),
                LoanProductId = Guid.Parse("01948273-3333-4444-8888-000000000003"),
                DocumentTypeCode = "AUDITED_ACCTS",
                DocumentTypeName = "Audited Accounts",
                Description = "Last 2 years audited financial statements signed by a certified accountant.",
                IsMandatory = true,
                MaxFileSizeMb = 10,
                AllowedFileTypes = "pdf"
            },
            new DocumentRequirement
            {
                Id = Guid.Parse("11948273-3333-0000-0000-000000000004"),
                LoanProductId = Guid.Parse("01948273-3333-4444-8888-000000000003"),
                DocumentTypeCode = "VALID_ID",
                DocumentTypeName = "Director's ID",
                Description = "Valid government ID of at least one director.",
                IsMandatory = true,
                MaxFileSizeMb = 2,
                AllowedFileTypes = "pdf,jpg,png"
            },

            // --- EMERGENCY LOAN REQUIREMENTS (01948273-4444-...) ---
            new DocumentRequirement
            {
                Id = Guid.Parse("11948273-4444-0000-0000-000000000001"),
                LoanProductId = Guid.Parse("01948273-4444-4444-8888-000000000004"),
                DocumentTypeCode = "VALID_ID",
                DocumentTypeName = "Government Issued ID",
                Description = "Passport, Driver's License or National ID for immediate verification.",
                IsMandatory = true,
                MaxFileSizeMb = 2,
                AllowedFileTypes = "pdf,jpg,png"
            }
        );

            builder.Entity<LoanProducts>().HasData(
            new LoanProducts
            {
                Id = Guid.Parse("01948273-1111-4444-8888-000000000001"),
                ProductCode = "PLN001",
                Name = "Personal Loan",
                LoanType = LoanType.Personal,
                MinAmount = 50000m,
                MaxAmount = 50000000m,
                MinTenorMonths = 6,
                MaxTenorMonths = 60,
                InterestRatePercent = 18.00m,
                InterestRateType = InterestRateType.Fixed,
                MaxLTIMultiplier = 60.00m, // 5x Annual = 60x Monthly
                MaxDSRPercent = 33.00m,
                RequiredDocumentTypes = "[\"GovID\", \"UtilityBill\", \"3MonthsPayslip\"]",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            },
            new LoanProducts
            {
                Id = Guid.Parse("01948273-2222-4444-8888-000000000002"),
                ProductCode = "SADV001",
                Name = "Salary Advance",
                LoanType = LoanType.SalaryAdvance,
                MinAmount = 10000m,
                MaxAmount = 1000000m,
                MinTenorMonths = 1,
                MaxTenorMonths = 12,
                InterestRatePercent = 5.00m,
                InterestRateType = InterestRateType.Flat,
                MaxLTIMultiplier = 3.00m, // 3x Monthly
                MaxDSRPercent = 40.00m,
                RequiredDocumentTypes = "[\"CompanyID\", \"1MonthPayslip\"]",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            },
            new LoanProducts
            {
                Id = Guid.Parse("01948273-3333-4444-8888-000000000003"),
                ProductCode = "BIZ001",
                Name = "Business Loan",
                LoanType = LoanType.Business,
                MinAmount = 500000m,
                MaxAmount = 100000000m,
                MinTenorMonths = 12,
                MaxTenorMonths = 84,
                InterestRatePercent = 22.00m,
                InterestRateType = InterestRateType.Variable,
                MaxLTIMultiplier = 0.00m, // Turnover-based logic
                MaxDSRPercent = 35.00m,
                RequiredDocumentTypes = "[\"CAC_Docs\", \"6MonthsBankStatement\"]",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            },
            new LoanProducts
            {
                Id = Guid.Parse("01948273-4444-4444-8888-000000000004"),
                ProductCode = "EMG001",
                Name = "Emergency Loan",
                LoanType = LoanType.Emergency,
                MinAmount = 5000m,
                MaxAmount = 100000m,
                MinTenorMonths = 1,
                MaxTenorMonths = 6,
                InterestRatePercent = 10.00m,
                InterestRateType = InterestRateType.Flat,
                MaxLTIMultiplier = 1.00m, // 1x Monthly
                MaxDSRPercent = 50.00m,
                RequiredDocumentTypes = "[\"GovID\"]",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            }
        );

            builder.Entity<ApplicantEmployment>()
                .Property(e => e.MonthlyGrossSalary)
                .HasPrecision(18, 2);

            builder.Entity<ApplicantEmployment>()
                .Property(e => e.MonthlyNetSalary)
                .HasPrecision(18, 2);

            

        }


    }
}
