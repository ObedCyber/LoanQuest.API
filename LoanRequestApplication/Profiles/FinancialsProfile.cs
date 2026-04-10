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
    public class FinancialsProfile : Profile
    {
        public FinancialsProfile() 
        {
            CreateMap<FinancialsRequest, ApplicantFinancials>();
            CreateMap<ApplicantFinancials, FinancialsResponse>();
        }
    }
}
