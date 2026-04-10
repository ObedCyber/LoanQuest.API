using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestDomain.Enums;
using Microsoft.AspNetCore.Identity;

namespace LoanRequestDomain.Entities
{
    public class Applicant
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(20)]
        public string? MaritalStatus { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }

        [StringLength(100)]
        public string? StateOfOrigin { get; set; }

        [Required]
        [StringLength(500)]
        public string ResidentialAddress { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ResidentialState { get; set; }

        [StringLength(100)]
        public string? ResidentialLGA { get; set; }

        [StringLength(11)]
        public string BVN { get; set; } = string.Empty;

        [StringLength(11)]
        public string? NIN { get; set; }

        public KycStatus KycStatus { get; set; } = KycStatus.Pending;

        public DateTime? KycVerifiedAt { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal ProfileCompleteness { get; set; } = 0.00m;

        [StringLength(20)]
        public string? ReferralCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } = null!;

        public ApplicantFinancials? Financials { get; set; }
        public ApplicantEmployment? Employment { get; set; }
        public ICollection<EligibilityChecks>? EligibilityChecks { get; set; }
        public ICollection<LoanApplication> LoanApplications { get; set; } = [];
    }
}
