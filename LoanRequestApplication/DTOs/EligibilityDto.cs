using System.ComponentModel.DataAnnotations;

namespace LoanRequestApplication.DTOs
{
    public class EligibilityRequestDto
    {
        [Required]
        public Guid LoanProductId { get; set; }

        [Required]
        [Range(5000, 500000000, ErrorMessage = "Please enter a valid amount")]
        public decimal RequestedAmount { get; set; }

        [Required]
        [Range(1, 360, ErrorMessage = "Tenor must be between 1 and 360 months")]
        public int RequestedTenorMonths { get; set; }
    }

    public class EligibilityResultResponse : BaseResponse
    {
        public EligibilityResponseDto? Data { get; set; }
    }


    public class EligibilityResponseDto
    {
        public bool IsEligible { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal MinEligibleAmount { get; set; }
        public decimal MaxEligibleAmount { get; set; }
        public decimal RecommendedAmount { get; set; }
        public decimal MaxMonthlyRepayment { get; set; }
        public decimal EffectiveInterestRate { get; set; }
        public decimal DSRApplied { get; set; }
        public decimal DisposableIncome { get; set; }
        public decimal TotalRepayable { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string RiskRating { get; set; }
        public List<RejectionReason> RejectionReasons { get; set; } = new();
    }

    public class EligibilityCheckDetailDto : BaseResponse
    {
        public Guid Id { get; set; }
        public string LoanProductName { get; set; }
        public decimal RequestedAmount { get; set; }
        public int RequestedTenorMonths { get; set; }
        public bool IsEligible { get; set; }
        public decimal MaxEligibleAmount { get; set; }
        public decimal MonthlyRepayment { get; set; }
        public string? RiskRating { get; set; }
        public List<string> RejectionReasons { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }
    public class RejectionReason
    {
        public string? Code { get; set; }
        public string? Message { get; set; }

        // Static factory methods so rejection reasons are 
        public static RejectionReason InsufficientDisposableIncome() => new()
        {
            Code = "INSUFFICIENT_DISPOSABLE_INCOME",
            Message = "Your existing obligations exceed your disposable income."
        };

        public static RejectionReason TenorExceedsRetirementAge(int maxAllowedTenor) => new()
        {
            Code = "TENOR_EXCEEDS_RETIREMENT_AGE",
            Message = $"Loan tenor exceeds retirement age. Maximum allowed tenor is {maxAllowedTenor} months."
        };

        public static RejectionReason AmountBelowMinimum(decimal minAmount) => new()
        {
            Code = "AMOUNT_BELOW_PRODUCT_MINIMUM",
            Message = $"Your eligible amount is below the minimum loan amount of {minAmount:N2}."
        };

        public static RejectionReason RequestedAmountExceedsEligible(decimal maxAmount) => new()
        {
            Code = "REQUESTED_AMOUNT_EXCEEDS_ELIGIBLE",
            Message = $"Your requested amount exceeds your eligible range. Maximum available is {maxAmount:N2}."
        };

        public static RejectionReason ProductNotAvailableForTenor() => new()
        {
            Code = "TENOR_OUT_OF_PRODUCT_RANGE",
            Message = "The requested tenor is outside the allowed range for this product."
        };
    }
}
