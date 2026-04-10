using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRequestApplication.DTOs
{
    public class  LoanProductFetchResponse : BaseResponse
    {
     public IEnumerable<LoanProductResponseDto>? Data { get; set; }
    }
    public class LoanProductResponseDto
    {
        public Guid Id { get; init; }
        public string ProductCode { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string LoanType { get; init; } = string.Empty; // Enum as String
        public decimal MinAmount { get; init; }
        public decimal MaxAmount { get; init; }
        public int MinTenorMonths { get; init; }
        public int MaxTenorMonths { get; init; }
        public decimal InterestRatePercent { get; init; }
        public string InterestRateType { get; init; } = string.Empty; // Enum as String

        // We convert the JSON string from the DB into a real List for the Frontend
        public List<string> RequiredDocuments { get; init; } = new();
    }
}
