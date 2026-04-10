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
    public class ApplicantEmployment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ApplicantId { get; set; }

        [ForeignKey("ApplicantId")]
        public virtual Applicant Applicant { get; set; } = null!;

        [Required]        
        public EmploymentType EmploymentType { get; set; }

        [MaxLength(256)]
        public string? EmployerName { get; set; }

        [MaxLength(500)]
        public string? EmployerAddress { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? EmployerPhone { get; set; }

        [MaxLength(150)]
        public string? JobTitle { get; set; }

        [MaxLength(150)]
        public string? Department { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EmploymentStartDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyGrossSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyNetSalary { get; set; }

        [MaxLength(100)]
        public string? SalaryAccountBank { get; set; }

        [MaxLength(10)]
        public string? SalaryAccountNumber { get; set; }

        public bool IsCurrentEmployer { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
