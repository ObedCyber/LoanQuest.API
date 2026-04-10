using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRequestApplication.DTOs
{
        public class ProfileRequest
        {
            [Required(ErrorMessage = "First name is required")]
            [StringLength(100)]
            public string FirstName { get; init; } = string.Empty;

            [StringLength(100)]
            public string? MiddleName { get; init; }

            [Required(ErrorMessage = "Last name is required")]
            [StringLength(100)]
            public string LastName { get; init; } = string.Empty;

            [Required(ErrorMessage = "Date of birth is required")]
            public DateTime DateOfBirth { get; init; }

            [Required]
            [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Invalid Gender")]
            public string Gender { get; init; } = string.Empty;

            [StringLength(20)]
            public string? MaritalStatus { get; init; }

            [Required]
            public string Nationality { get; init; } = "Nigerian";

            [Required]
            public string StateOfOrigin { get; init; } = string.Empty;

            [Required]
            [StringLength(500)]
            public string ResidentialAddress { get; init; } = string.Empty;

            [Required]
            public string ResidentialState { get; init; } = string.Empty;

            [Required]
            public string ResidentialLGA { get; init; } = string.Empty;

            [StringLength(20)]
            public string? ReferralCode { get; init; }

            [StringLength(11, MinimumLength = 11)]
            public string? NIN { get; init; }
        }

        public class ProfileRegistrationResponse : BaseResponse
        {
            public ProfileSummary? Data { get; set; }
        }

        public class ProfileSummary
        {
            public Guid ApplicantId { get; init; }
            public string FullName { get; init; } = string.Empty;
            public string KycStatus { get; init; } = string.Empty;
            public decimal ProfileCompleteness { get; init; }
            public DateTime RegisteredAt { get; init; }
        }

        public class ApplicantProfileDetails
        {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty; // From the IdentityUser relationship
        public string? Gender { get; init; }
        public string? MaritalStatus { get; init; }
        public int Age { get; init; } // Calculated from DateOfBirth
        public string BVN { get; init; } = string.Empty;
        public string? NIN { get; init; }
        public string KycStatus { get; init; } = string.Empty;
        public decimal ProfileCompleteness { get; init; }
        public string ResidentialAddress { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty; // Combined State and LGA
        public DateTime RegisteredAt { get; init; }
    }

    public class ProfileUpdateRequest
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Invalid Gender")]
        public string? Gender { get; init; }

        public string? MaritalStatus { get; set; }

        [StringLength(500)]
        public string? ResidentialAddress { get; set; }

        public string? ResidentialState { get; set; }

        public string? ResidentialLGA { get; set; }
    }

    // Inheriting from BaseResponse to keep it DRY
    public class ProfileUpdateResponse : BaseResponse
    {
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }


}
