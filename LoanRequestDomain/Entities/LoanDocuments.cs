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
        [Table("LoanDocuments")]
        public class LoanDocument
        {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            [Required]
            public Guid LoanApplicationId { get; set; }

            [Required]
            public Guid ApplicantId { get; set; }

            [Required]
            [StringLength(50)]
            public required string DocumentTypeCode { get; set; } // e.g., PAYSLIP, BANK_STMT, ID

            [Required]
            [StringLength(150)]
            public string DocumentTypeName { get; set; } // e.g., "March 2026 Pay Slip"

            [Required]
            [StringLength(256)]
            public required string FileName { get; set; } // Original user filename

            [Required]
            [StringLength(1024)]
            public required string StoragePath { get; set; } // Azure Blob path/unique key (e.g., "loans/id/doc.pdf")

            [Required]
            public long FileSize { get; set; } // Maps perfectly to BIGINT in SQL

            [Required]
            [StringLength(100)]
            public string ContentType { get; set; } // MIME type (e.g., "application/pdf")

            [Required]
            [StringLength(20)]
            public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

            [StringLength(500)]
            public string? RejectionReason { get; set; } // Nullable by default in C#

            public Guid? ReviewedBy { get; set; } // Nullable foreign key

            public DateTime? ReviewedAt { get; set; } // Nullable DATETIME2

            [Required]
            public DateTime UploadedAt { get; set; } = DateTime.UtcNow; // Default to current UTC time

            public DateTime? ExpiresAt { get; set; } // Optional: For document lifecycle management

            [Required]
            public bool IsDeleted { get; set; } = false; // Soft delete flag

            // ==========================================
            // Navigation Properties (EF Core Relationships)
            // ==========================================

            [ForeignKey("LoanApplicationId")]
            public virtual required LoanApplication LoanApplication { get; set; }

            [ForeignKey("ApplicantId")]
            public virtual required Applicant Applicant { get; set; }

        }
   
}
