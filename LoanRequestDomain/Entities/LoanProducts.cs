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
    public class LoanProducts
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(20)]
        public string ProductCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public LoanType LoanType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxAmount { get; set; }

        public int MinTenorMonths { get; set; }

        public int MaxTenorMonths { get; set; }

        [Column(TypeName = "decimal(8,4)")]
        public decimal InterestRatePercent { get; set; }

        [Required]
        public InterestRateType InterestRateType { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal MaxLTIMultiplier { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal MaxDSRPercent { get; set; } = 33.00m;

        [Required]
        public string RequiredDocumentTypes { get; set; } = "[]"; // Store as JSON string: ["ID", "UtilityBill"]

        public string? EligibilityCriteria { get; set; } // Store as JSON string for complex rules

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } 

        public DateTime? UpdatedAt { get; set; }

        public ICollection<DocumentRequirement> DocumentRequirements { get; set; } = [];
    }
}
