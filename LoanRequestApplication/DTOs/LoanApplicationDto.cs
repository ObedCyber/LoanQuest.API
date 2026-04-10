using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRequestApplication.DTOs
{
    public class LoanApplicationRequestDto
    {
        [Required(ErrorMessage = "Eligibility Check ID is required to proceed.")]
        public Guid EligibilityCheckId { get; set; }

        [Required(ErrorMessage = "Requested amount is required.")]
        public decimal RequestedAmount { get; set; }

        [Required(ErrorMessage = "Loan tenor (in months) is required.")]
        public int TenorMonths { get; set; }

        [Required(ErrorMessage = "Please state the purpose of this loan.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Purpose must be between 10 and 500 characters.")]
        public string LoanPurpose { get; set; } = null!;
    }

    public class LoanApplicationResponseDto : BaseResponse
    {
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal RequestedAmount { get; set; }
        public decimal MonthlyRepayment { get; set; }
        public decimal TotalRepayable { get; set; }
        public List<DocumentChecklistItemDto> DocumentChecklist { get; set; } = new();
    
    }

    public class DocumentChecklistItemDto
    {
        public string DocumentTypeCode { get; set; } = null!;
        public string DocumentTypeName { get; set; } = null!;
        public bool IsMandatory { get; set; }
        public string Status { get; set; } = null!; // e.g., "Pending", "Uploaded"
        public Guid? UploadedDocumentId { get; set; } // Linked from LoanDocumentId
    }

    public class LoanApplicationSummaryDto
    {
        public Guid Id { get; set; }
        public string ApplicationNumber { get; set; } = null!;
        public string LoanProductName { get; set; } = null!; 
        public decimal RequestedAmount { get; set; }
        public int TenorMonths { get; set; }
        public string Status { get; set; } = null!; 
        public DateTime SubmittedAt { get; set; }
    }

    public class LoanApplicationListResponseDto
    {
        public int TotalCount { get; set; }
        public IEnumerable<LoanApplicationSummaryDto> Applications { get; set; } = [];
    }

    public class LoanApplicationDetailResponseDto : BaseResponse
    {
        public Guid Id { get; set; }
        public string ApplicationNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? StatusReason { get; set; } 

        public string ProductName { get; set; } = null!;
        public string ProductCode { get; set; } = null!;

        public decimal RequestedAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public int TenorMonths { get; set; }
        public decimal InterestRate { get; set; }
        public decimal MonthlyRepayment { get; set; }
        public decimal TotalRepayable { get; set; }
        public string? LoanPurpose { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? DecisionAt { get; set; }

        public bool ConsentGiven { get; set; }
        public DateTime? ConsentAt { get; set; }
        public string? BlacklistResult { get; set; } 
    }

    public class LoanApplicationUpdateDto
    {
        [Range(5000, 50000000)]
        public decimal? RequestedAmount { get; set; }

        [Range(1, 360)]
        public int? TenorMonths { get; set; }

        [StringLength(500, MinimumLength = 10)]
        public string? LoanPurpose { get; set; }
    }

    public class LoanUpdateResponseDto : BaseResponse
    {
        public Guid Id { get; set; }
        public string ApplicationNumber { get; set; } = null!;
        public string Status { get; set; } = null!;

        // Updated Financials
        public decimal RequestedAmount { get; set; }
        public int TenorMonths { get; set; }
        public decimal MonthlyRepayment { get; set; }
        public decimal TotalRepayable { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class LoanDeleteResponseDto : BaseResponse
    {
        public Guid Id { get; set; }
        public string ApplicationNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime DeletedAt { get; set; }
    }
}
