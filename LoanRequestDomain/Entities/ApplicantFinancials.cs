using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRequestDomain.Entities
{
    public class ApplicantFinancials
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ApplicantId { get; set; }

        [ForeignKey("ApplicantId")]
        public virtual Applicant Applicant { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyObligations { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherMonthlyIncome { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAssets { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalLiabilities { get; set; }

        public int? CreditScore { get; set; }

        [MaxLength(100)]
        public string? CreditBureauRef { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
