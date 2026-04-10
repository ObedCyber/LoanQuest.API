using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LoanRequestApplication.DTOs;
using LoanRequestDomain.Entities;

namespace LoanRequestApplication.Profiles
{
    public class ApplicantProfile : Profile
    {
        public ApplicantProfile() 
        {
            CreateMap<ProfileRequest, Applicant>();

            CreateMap<Applicant, ApplicantProfileDetails>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                $"{src.FirstName} {src.MiddleName} {src.LastName}".Replace("  ", " ")))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src =>
                $"{src.ResidentialLGA}, {src.ResidentialState}"))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
                DateTime.Today.Year - src.DateOfBirth.Year -
                (src.DateOfBirth.Date > DateTime.Today.AddYears(-(DateTime.Today.Year - src.DateOfBirth.Year)) ? 1 : 0)))
            .ForMember(dest => dest.RegisteredAt, opt => opt.MapFrom(src => src.CreatedAt));

            CreateMap<ProfileUpdateRequest, Applicant>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
