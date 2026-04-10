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
    public class LoanApplication
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string ApplicationNumber { get; set; } = null!; // Format: LQ-2026-000001

        [Required]
        public Guid ApplicantId { get; set; }

        [ForeignKey("ApplicantId")]
        public virtual Applicant Applicant { get; set; } = null!;

        [Required]
        public Guid LoanProductId { get; set; }

        [ForeignKey("LoanProductId")]
        public virtual LoanProducts LoanProduct { get; set; } = null!;

        [Required]
        public Guid EligibilityCheckId { get; set; }

        [ForeignKey("EligibilityCheckId")]
        public virtual EligibilityChecks EligibilityCheck { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RequestedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ApprovedAmount { get; set; }

        [Required]
        public int TenorMonths { get; set; }

        [Required]
        [Column(TypeName = "decimal(8,4)")]
        public decimal InterestRate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyRepayment { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalRepayable { get; set; }

        [MaxLength(500)]
        public string? LoanPurpose { get; set; }

        [Required]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;

        [MaxLength(500)]
        public string? StatusReason { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? DecisionAt { get; set; }

        public Guid? DecisionBy { get; set; }

        public bool ConsentGiven { get; set; } = false;

        public DateTime? ConsentAt { get; set; }

        public string? PreScreeningResult { get; set; } // JSON: auto-rules outcome

        public bool BlacklistChecked { get; set; } = false;

        [MaxLength(20)]
        public string? BlacklistResult { get; set; }

        public int? BureauScoreUsed { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<ApplicationDocumentChecklist>? DocumentChecklist { get; set; }
    }
}
