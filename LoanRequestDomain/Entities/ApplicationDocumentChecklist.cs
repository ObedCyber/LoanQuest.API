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
    public class ApplicationDocumentChecklist
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid LoanApplicationId { get; set; }
        public required string DocumentTypeCode { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public string AllowedFileTypes { get; set; } = string.Empty;
        public int MaxFileSizeMb { get; set; }
        public ChecklistItemStatus Status { get; set; } = ChecklistItemStatus.Pending;
        public Guid? LoanDocumentId { get; set; }       
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation
        [ForeignKey("LoanApplicationId")]
        public virtual required LoanApplication LoanApplication { get; set; }
    }
}
