using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRequestApplication.DTOs
{
    public class BvnKycRequest
    {
        [StringLength(11)]
        public string Bvn { get; init; } = string.Empty;
    }

    public class BvnKycResponse : BaseResponse
    {

    }

    public class BVNValidationRequest
    {
        public string Bvn { get; init; } = string.Empty;

        public string Firstname { get; init; } = string.Empty;

        public string Lastname { get; init; } = string.Empty;

        public string RedirectUrl { get; init; } = string.Empty;
    }

    public class BvnVerificationResponse
    {
        public string Status { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public BvnData? Data { get; init; } = new();
    }

    public class BvnData
    {
        public string Url { get; init; } = string.Empty;
        public string Reference { get; init; } = string.Empty;
    }
}
