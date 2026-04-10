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
    public class EmploymentProfile : Profile
    {
        public EmploymentProfile() 
        {
            CreateMap<ApplicantEmployment, EmploymentDetailsDto>()
            .ForMember(dest => dest.EmploymentType, opt => opt.MapFrom(src => src.EmploymentType.ToString()));

            CreateMap<EmploymentRequest, ApplicantEmployment>();
        }
    }
}
