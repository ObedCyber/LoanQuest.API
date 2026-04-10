using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LoanRequestApplication.DTOs;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestApplication.Interfaces.Services;
using LoanRequestDomain.Entities;
using Microsoft.AspNetCore.Http;

namespace LoanRequestInfrastructure.Services.Applicants
{
    public class ApplicantService : BaseService, IApplicantService
    {
       // private readonly IGenericRepository<Applicant> _repository;
        private readonly IApplicantRepository _repository;
        private readonly IApplicantFinancialsRepository _applicantFinancialsRepository;
        private readonly IMapper _mapper;
        public ApplicantService(IApplicantRepository repository, IApplicantFinancialsRepository applicantFinancialsRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository = repository;
            _applicantFinancialsRepository = applicantFinancialsRepository;
            _mapper = mapper;
        }

        public async Task<ProfileRegistrationResponse> RegisterApplicantAsync(ProfileRequest request)
        {
            bool applicantExists = await _repository.CheckApplicantWithUserIdExist(CurrentUserId!);
            if(applicantExists)
            {
                return new ProfileRegistrationResponse
                {
                    IsSuccess = false,
                    Message = "Profile already exists for this user."
                };
            }
            var ApplicantProfile = _mapper.Map<Applicant>(request);

            ApplicantProfile.CreatedAt = DateTime.UtcNow;
            ApplicantProfile.UserId = CurrentUserId!;

            await _repository.AddAsync(ApplicantProfile);
            bool saved = await _repository.SaveChangesAsync();

            if (!saved)
            {
                return new ProfileRegistrationResponse { IsSuccess = false, Message = "Failed to save profile." };
            }

            return new ProfileRegistrationResponse
            {
                IsSuccess = true,
                Message = "Profile registration successful.",
                Data = new ProfileSummary
                {
                    ApplicantId = ApplicantProfile.Id,
                    FullName = $"{ApplicantProfile.FirstName} {ApplicantProfile.LastName}",
                    KycStatus = ApplicantProfile.KycStatus.ToString(),
                    ProfileCompleteness = ApplicantProfile.ProfileCompleteness, 
                    RegisteredAt = ApplicantProfile.CreatedAt
                }
            };

        }

        public async Task<ApplicantProfileDetails?> GetApplicantProfileAsync()
        {
            var result = await _repository.GetApplicantByUserIdAsync(CurrentUserId);
            if (result == null) return  null;
            var applicantProfile = _mapper.Map<ApplicantProfileDetails>(result);

            return applicantProfile;
        }

        public async Task<ProfileUpdateResponse> UpdateApplicantAsync(ProfileUpdateRequest request)
        {
            var existingApplicant = await _repository.GetApplicantByUserIdAsync(CurrentUserId);

            if (existingApplicant == null)
            {
                return BaseResponse.Failure<ProfileUpdateResponse>("Applicant profile not found.");
            }

            _mapper.Map(request, existingApplicant);
         
             existingApplicant.UpdatedAt = DateTime.UtcNow; 
            
            _repository.Update(existingApplicant); 
            bool saved = await _repository.SaveChangesAsync();

            if (!saved)
            {
                return BaseResponse.Failure<ProfileUpdateResponse>("No changes were saved or update failed.");
            }
            
            var response = BaseResponse.Success<ProfileUpdateResponse>("Profile updated successfully.");
            response.UpdatedAt = DateTime.UtcNow;
            return response;
        }

        public async Task<FinancialsResponse> AddApplicantFinancialDetails (FinancialsRequest request)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                return BaseResponse.Failure<FinancialsResponse>("User session is invalid.");
            }
            var applicant = await _repository.GetApplicantByUserIdAsync(CurrentUserId);
            if (applicant == null) return BaseResponse.Failure<FinancialsResponse>("Applicant profile not found.");
            if (applicant.BVN == string.Empty) return BaseResponse.Failure<FinancialsResponse>("Applicant BVN not found, Please Verify BVN Number.");
            MockCreditReport creditReport = await GetMockCreditProfile(applicant.BVN);
            if (!creditReport.IsSuccess) return BaseResponse.Failure<FinancialsResponse>($"Credit Score Verification Failed, BVN Number: {applicant.BVN}");
            var financialEntity = _mapper.Map<ApplicantFinancials>(request);
            var existingFinancials = await _applicantFinancialsRepository.GetFinancialsByApplicantIdAsync(applicant.Id);

            if (existingFinancials != null) return BaseResponse.Failure<FinancialsResponse>("Financial details already exist for this applicant. Please update existing details instead.");
            financialEntity.ApplicantId = applicant.Id;
            try
            {
                financialEntity.MonthlyObligations = request.MonthlyObligations;
                financialEntity.OtherMonthlyIncome = request.OtherMonthlyIncome;
                financialEntity.TotalAssets = request.TotalAssets;
                financialEntity.TotalLiabilities = request.TotalLiabilities;
                financialEntity.CreditScore = creditReport.Score;
                financialEntity.CreditBureauRef = creditReport.Reference;

                await _applicantFinancialsRepository.AddAsync(financialEntity);
                bool saved = await _applicantFinancialsRepository.SaveChangesAsync();
                if (!saved)
                {
                    return BaseResponse.Failure<FinancialsResponse>("Failed to save financial details.");
                }
                FinancialsResponse responseData = _mapper.Map<FinancialsResponse>(financialEntity);
                responseData.IsSuccess = true;
                responseData.Message = "Financial details added successfully.";
                return responseData;
            }
            catch (Exception ex)
            {
                return BaseResponse.Failure<FinancialsResponse>($"An error occurred while saving financial details: {ex.Message}");
            }
        }

        public static async Task<MockCreditReport> GetMockCreditProfile(string bvn)
        {
            // Simulate API Latency
            await Task.Delay(1000);

           // if BVN starts with '0', simulate a fail/no-record
            if (bvn.StartsWith((char)0))
            {
                return new MockCreditReport { IsSuccess = false };
            }

            var random = new Random();
            return new MockCreditReport
            {
                IsSuccess = true,
                // Most Credit Bureau scores range from 300 to 850
                Score = random.Next(450, 800),
                Reference = $"MOCK-CRC-{Guid.NewGuid().ToString("N").ToUpper()[..8]}"
            };
        }

    }
}
