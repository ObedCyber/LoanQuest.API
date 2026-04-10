using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRequestApplication.DTOs
{
    public class FinancialsRequest
    {
        [Range(0, double.MaxValue, ErrorMessage = "Monthly obligations cannot be negative")]
        public decimal MonthlyObligations { get; init; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Other monthly income cannot be negative")]
        public decimal OtherMonthlyIncome { get; init; } = 0;

        public decimal? TotalAssets { get; init; }

        public decimal? TotalLiabilities { get; init; }
    }

    public class FinancialsResponse : BaseResponse
    {
        public Guid Id { get; init; }
        public decimal MonthlyObligations { get; init; }
        public decimal OtherMonthlyIncome { get; init; }
        public decimal? TotalAssets { get; init; }
        public decimal? TotalLiabilities { get; init; }

        // System-generated fields
        public int? CreditScore { get; init; }
        public string? CreditBureauRef { get; init; }
        public DateTime? CreditPulledAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
