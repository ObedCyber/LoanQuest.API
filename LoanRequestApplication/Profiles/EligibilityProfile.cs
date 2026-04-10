using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using LoanRequestApplication.DTOs;
using LoanRequestDomain.Entities;
using LoanRequestDomain.Enums;

namespace LoanRequestApplication.Profiles
{
    public class EligibilityProfile : Profile
    {
        public EligibilityProfile() {
            CreateMap<EligibilityResponseDto, EligibilityChecks>()
            // 1. Convert List<RejectionReason> to a JSON string
            .ForMember(dest => dest.RejectionReasons, opt => opt.MapFrom(src =>
                src.RejectionReasons != null && src.RejectionReasons.Any()
                ? JsonSerializer.Serialize(src.RejectionReasons, (JsonSerializerOptions)null)
                : null))

            // 2. Convert string to RiskRating Enum
            .ForMember(dest => dest.RiskRating, opt => opt.MapFrom(src =>
                Enum.Parse<RiskRating>(src.RiskRating, true)))

            // 3. Ignore fields that aren't in the DTO (calculated in Service)
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ApplicantId, opt => opt.Ignore())
            .ForMember(dest => dest.LoanProductId, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedTenorMonths, opt => opt.Ignore())
            .ForMember(dest => dest.MonthlyGrossSalary, opt => opt.Ignore())
            .ForMember(dest => dest.MonthlyObligations, opt => opt.Ignore())
            .ForMember(dest => dest.IpAddress, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Applicant, opt => opt.Ignore())
            .ForMember(dest => dest.LoanProduct, opt => opt.Ignore());

            CreateMap<EligibilityChecks, EligibilityCheckDetailDto>()
            .ForMember(dest => dest.LoanProductName, opt => opt.MapFrom(src => src.LoanProduct.Name))
            .ForMember(dest => dest.RejectionReasons, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.RejectionReasons)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(src.RejectionReasons, (JsonSerializerOptions)null)))
            .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.ExpiresAt < DateTime.UtcNow))
            .ForMember(dest => dest.MonthlyRepayment, opt => opt.MapFrom(src => src.MaxMonthlyRepayment));

            CreateMap<EligibilityChecks, EligibilityResponseDto>()
            .ForMember(dest => dest.DSRApplied, opt => opt.MapFrom(src => src.DSRApplied))
            .ForMember(dest => dest.EffectiveInterestRate, opt => opt.MapFrom(src => src.EffectiveInterestRate))
            .ForMember(dest => dest.RejectionReasons, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.RejectionReasons)
                ? new List<RejectionReason>()
                : JsonSerializer.Deserialize<List<RejectionReason>>(src.RejectionReasons, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })))

            // Ensure the currency values match
            .ForMember(dest => dest.MaxMonthlyRepayment, opt => opt.MapFrom(src => src.MaxMonthlyRepayment))
             .ForMember(dest => dest.TotalRepayable, opt => opt.Ignore());
        }
    }
}
