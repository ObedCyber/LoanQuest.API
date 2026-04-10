using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.DTOs;

namespace LoanRequestApplication.Interfaces.Services
{
    public interface IApplicantService
    {
        Task<ProfileRegistrationResponse> RegisterApplicantAsync(ProfileRequest request);
        Task<ApplicantProfileDetails?> GetApplicantProfileAsync();
        Task<ProfileUpdateResponse> UpdateApplicantAsync(ProfileUpdateRequest request);
        Task<FinancialsResponse> AddApplicantFinancialDetails(FinancialsRequest request);
    }
}
