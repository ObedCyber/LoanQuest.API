using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRequestDomain.Entities
{
    public class DocumentRequirement
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid LoanProductId { get; set; }

        [Required]
        public required string DocumentTypeCode { get; set; }  // PAYSLIP, BANK_STMT, EMP_LETTER, ID
        public string DocumentTypeName { get; set; } = string.Empty;  // "Last 3 Months Payslip"
        public string Description { get; set; } = string.Empty;      // "Must show employer name and salary"
        public bool IsMandatory { get; set; }         // true = blocks submission if missing
        public int MaxFileSizeMb { get; set; }  // e.g. 5
        [Required]
        public required string AllowedFileTypes { get; set; }  // "pdf,jpg,png" — stored as CSV or JSON

        // Navigation
        [ForeignKey("LoanProductId")]
        public virtual LoanProducts LoanProduct { get; set; } = null!;
    }
}
