using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestDomain.Enums;

namespace LoanRequestDomain.Entities
{
    public class EligibilityChecks
    {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            [Required]
            public Guid ApplicantId { get; set; }

            [ForeignKey("ApplicantId")]
            public virtual Applicant Applicant { get; set; } = null!;

            [Required]
            public Guid LoanProductId { get; set; }

            [ForeignKey("LoanProductId")]
            public virtual LoanProducts LoanProduct { get; set; } = null!;

            [Column(TypeName = "decimal(18,2)")]
            public decimal RequestedAmount { get; set; }

            public int RequestedTenorMonths { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal MonthlyGrossSalary { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal MonthlyObligations { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal DisposableIncome { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal MaxEligibleAmount { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal MinEligibleAmount { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal RecommendedAmount { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal MaxMonthlyRepayment { get; set; }

            [Column(TypeName = "decimal(8,4)")]
            public decimal EffectiveInterestRate { get; set; }

            [Column(TypeName = "decimal(5,2)")]
            public decimal DSRApplied { get; set; }

            public bool IsEligible { get; set; }

            public string? RejectionReasons { get; set; } // JSON array: ["INSUFFICIENT_INCOME", "DSR_EXCEEDED"]

           public RiskRating RiskRating { get; set; } // Low, Medium, High, Decline

            public DateTime ExpiresAt { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            [MaxLength(45)]
            public string? IpAddress { get; set; }
        
    }
}
