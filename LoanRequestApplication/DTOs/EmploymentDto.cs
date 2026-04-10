using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LoanRequestDomain.Enums;

namespace LoanRequestApplication.DTOs
{
    public class EmploymentRequest
    {
        [Required(ErrorMessage = "Employment type is required")]
        [EnumDataType(typeof(EmploymentType), ErrorMessage = "Invalid Employment Type")]
        public EmploymentType EmploymentType { get; init; }

        [Required(ErrorMessage = "Employer name is required")]
        [StringLength(256)]
        public string EmployerName { get; init; } = string.Empty;

        [StringLength(500)]
        public string? EmployerAddress { get; init; }

        [Phone]
        [StringLength(20)]
        public string? EmployerPhone { get; init; }

        [StringLength(150)]
        public string? JobTitle { get; init; }

        [StringLength(150)]
        public string? Department { get; init; }

        public DateTime? EmploymentStartDate { get; init; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value")]
        public decimal MonthlyGrossSalary { get; init; }

        public decimal? MonthlyNetSalary { get; init; }

        [StringLength(100)]
        public string? SalaryAccountBank { get; init; }

        [StringLength(10, MinimumLength = 10, ErrorMessage = "Account number must be 10 digits")]
        public string? SalaryAccountNumber { get; init; }

        public bool IsCurrentEmployer { get; init; } = true;
    }

    public class EmploymentResponse : BaseResponse
    {
        public EmploymentDetailsDto? Data { get; set; }
    }

    public class EmploymentDetailsDto
    {
        public Guid Id { get; init; }
        public string EmploymentType { get; init; } = string.Empty;
        public string EmployerName { get; init; } = string.Empty;
        public string? JobTitle { get; init; }
        public decimal MonthlyGrossSalary { get; init; }
        public string? SalaryAccountBank { get; init; }

        // Masked for security: shows only the last 3 digits
        public string? MaskedAccountNumber => string.IsNullOrEmpty(SalaryAccountNumber)
            ? null
            : $"*******{SalaryAccountNumber[^3..]}";
        [JsonIgnore]
        public string? SalaryAccountNumber { get; set; }
        public bool IsCurrentEmployer { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public class MockCreditReport
    {
        public int Score { get; set; }
        public string Reference { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}
