using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using LoanRequestApplication.DTOs;
using LoanRequestDomain.Entities;

namespace LoanRequestApplication.Profiles
{
    public class LoanProfile : Profile
    {
        public LoanProfile()
        {
            CreateMap<LoanProducts, LoanProductResponseDto>()
              .ForMember(dest => dest.LoanType, opt => opt.MapFrom(src => src.LoanType.ToString()))
              .ForMember(dest => dest.InterestRateType, opt => opt.MapFrom(src => src.InterestRateType.ToString()))
              .ForMember(dest => dest.RequiredDocuments, opt => opt.MapFrom(src =>
                  string.IsNullOrEmpty(src.RequiredDocumentTypes)
                  ? new List<string>()
                  : JsonSerializer.Deserialize<List<string>>(src.RequiredDocumentTypes, (JsonSerializerOptions)null)));

            CreateMap<LoanApplication, LoanApplicationResponseDto>()
                .ForMember(dest => dest.ApplicationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ApplicationNumber, opt => opt.MapFrom(src => src.ApplicationNumber))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.RequestedAmount, opt => opt.MapFrom(src => src.RequestedAmount))
                .ForMember(dest => dest.MonthlyRepayment, opt => opt.MapFrom(src => src.MonthlyRepayment))
                .ForMember(dest => dest.TotalRepayable, opt => opt.MapFrom(src => src.TotalRepayable));

            CreateMap<LoanApplication, LoanApplicationSummaryDto>()
                .ForMember(dest => dest.LoanProductName, opt => opt.MapFrom(src => src.LoanProduct.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.SubmittedAt, opt => opt.MapFrom(src => src.SubmittedAt ?? src.CreatedAt));

            CreateMap<LoanApplication, LoanApplicationDetailResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.LoanProduct.Name))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.LoanProduct.ProductCode))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<LoanApplication, LoanUpdateResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.MonthlyRepayment, opt => opt.MapFrom(src => Math.Round(src.MonthlyRepayment, 2)));
        }
    }
}
