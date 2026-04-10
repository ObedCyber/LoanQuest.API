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
using LoanRequestDomain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LoanRequestInfrastructure.Services.Loans
{
    public class LoanApplicationService : BaseService, ILoanApplicationService
    {
        private readonly ILoanApplicationRepository _loanApplicationRepository;
        private readonly IEligibilityChecksRepository _eligibilityRepository;
        private readonly IDocumentChecklistService _documentChecklistService;
        private readonly IGenericRepository<LoanProducts> _loanProductRepository;
        private readonly IMapper _mapper;

        public LoanApplicationService(
            ILoanApplicationRepository loanApplicationRepository,
            IDocumentChecklistService documentServiceChecklist,
            IMapper mapper,
            IEligibilityChecksRepository eligibilityRepository,
            IGenericRepository<LoanProducts> loanProductRepository,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _loanApplicationRepository = loanApplicationRepository;
            _mapper = mapper;
            _documentChecklistService = documentServiceChecklist;
            _loanProductRepository = loanProductRepository;
            _eligibilityRepository = eligibilityRepository;
        }

        public async Task<LoanApplicationResponseDto> CreateLoanApplication(LoanApplicationRequestDto request)
        {
            var check = await _eligibilityRepository.GetByIdAsync(request.EligibilityCheckId);
            if (check == null) return new LoanApplicationResponseDto { IsSuccess = false, Message = "Check Id does not exist." };
            if (check.ApplicantId != CurrentApplicantId) return BaseResponse.Failure<LoanApplicationResponseDto>("Unauthorized access to this Check");
            if (!check.IsEligible) return BaseResponse.Failure<LoanApplicationResponseDto>("Applicant is Ineligible for this Loan and cannot Proceed further");
            if (check.ExpiresAt < DateTime.UtcNow) return BaseResponse.Failure<LoanApplicationResponseDto>("Your eligibility check has expired. Please run a new check.");
            if (request.RequestedAmount < check.MinEligibleAmount) return BaseResponse.Failure<LoanApplicationResponseDto>($"Requested amount is below the minimum of {check.MinEligibleAmount:N2}.");
            if (request.RequestedAmount > check.MaxEligibleAmount) return BaseResponse.Failure<LoanApplicationResponseDto>($"Requested amount is above the maximum of {check.MaxEligibleAmount:N2}.");
            var loanProduct = await _loanProductRepository.GetByIdAsync(check.LoanProductId);
            if(request.TenorMonths < loanProduct!.MinTenorMonths) return BaseResponse.Failure<LoanApplicationResponseDto>($"Requested tenor is below the minimum of {loanProduct.MinTenorMonths} months.");
            if(request.TenorMonths > loanProduct.MaxTenorMonths) return BaseResponse.Failure<LoanApplicationResponseDto>($"Requested tenor is above the maximum of {loanProduct.MaxTenorMonths} months.");
            var existingApp = await _loanApplicationRepository.Query()
            .FirstOrDefaultAsync(x => x.EligibilityCheckId == request.EligibilityCheckId);

            if (existingApp != null)
            {
                var existingChecklist = await _documentChecklistService.GetChecklistForApplication(existingApp.Id);
                var response = _mapper.Map<LoanApplicationResponseDto>(existingApp);
                response.DocumentChecklist = _mapper.Map<List<DocumentChecklistItemDto>>(existingChecklist);
                response.IsSuccess = false;
                response.Message = "You already have a draft for this loan. Please continue your application.";
                return response;
            }
            decimal monthlyRepayment;
            decimal totalRepayable;
 
            if (request.RequestedAmount == check.RequestedAmount && request.TenorMonths == check.RequestedTenorMonths)
            {
                monthlyRepayment = check.MaxMonthlyRepayment;
                totalRepayable = monthlyRepayment * check.RequestedTenorMonths;
            }
            else
            {
                monthlyRepayment = CalculateMonthlyRepayment(request.RequestedAmount, check.EffectiveInterestRate, request.TenorMonths);
                totalRepayable = monthlyRepayment * request.TenorMonths;
            }

            var application = new LoanApplication
            {
                ApplicationNumber = await GenerateApplicationNumber(), 
                ApplicantId = check.ApplicantId,
                LoanProductId = check.LoanProductId,
                EligibilityCheckId = check.Id,
                RequestedAmount = request.RequestedAmount,
                TenorMonths = request.TenorMonths,
                InterestRate = check.EffectiveInterestRate, 
                MonthlyRepayment = monthlyRepayment,
                TotalRepayable = totalRepayable,
                LoanPurpose = request.LoanPurpose,
                Status = ApplicationStatus.Draft,
                SubmittedAt = null,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _loanApplicationRepository.AddAsync(application);
                await _loanApplicationRepository.SaveChangesAsync();
                bool checklistCreated = await _documentChecklistService.CreateChecklistForApplication(application.Id, check.LoanProductId);

                if (!checklistCreated)
                {
                    _loanApplicationRepository.Delete(application);
                    await _loanApplicationRepository.SaveChangesAsync();
                    return BaseResponse.Failure<LoanApplicationResponseDto>(
                        "Application created but document checklist could not be generated. Please try again."
                    );
                }
                var createdChecklist = await _documentChecklistService.GetChecklistForApplication(application.Id);
                var checklistDto = _mapper.Map<List<DocumentChecklistItemDto>>(createdChecklist);
                var response = _mapper.Map<LoanApplicationResponseDto>(application);
                response.IsSuccess = true;
                response.Message = "Loan application draft created successfully. Please upload your documents and submit.";
                response.DocumentChecklist = checklistDto;
                return response;
            }
            catch (Exception)
            {
                return BaseResponse.Failure<LoanApplicationResponseDto>("An error occurred while saving your application.");
            }
        }

        public async Task<LoanApplicationListResponseDto> GetAllLoanApplicationsForApplicant()
        {
            var loanApplications = await _loanApplicationRepository.GetAllLoanApplicationsForApplicantAsync(CurrentApplicantId);
            var response =_mapper.Map<IEnumerable<LoanApplicationSummaryDto>>(loanApplications);
            return new LoanApplicationListResponseDto
            {
                TotalCount = response.Count(),
                Applications = response
            };
        }

        public async Task<LoanApplicationDetailResponseDto> GetApplicationDetailAsync(Guid id)
        {
            var application = await _loanApplicationRepository.GetLoanApplicationWithDetailsAsync(id, CurrentApplicantId);
            if (application == null)
                return BaseResponse.Failure<LoanApplicationDetailResponseDto>("Application not found or you do not have access to it.");
            var response = _mapper.Map<LoanApplicationDetailResponseDto>(application);
            return response;
        }

        public async Task<LoanUpdateResponseDto> UpdateDraftApplication(Guid id, LoanApplicationUpdateDto request)
        {
            var application = await _loanApplicationRepository.GetLoanApplicationWithDetailsAsync(id, CurrentApplicantId);
            if (application == null)
                return BaseResponse.Failure<LoanUpdateResponseDto>("Application not found or you do not have access to it.");
            if (application.Status != ApplicationStatus.Draft) return BaseResponse.Failure<LoanUpdateResponseDto>("Only applications in Draft status can be updated.");

            if (request.RequestedAmount.HasValue)
            {
                var check = await _eligibilityRepository.GetByIdAsync(application.EligibilityCheckId);
                if (check == null)
                    return BaseResponse.Failure<LoanUpdateResponseDto>("Eligibility check not found.");

                if (request.RequestedAmount.Value < check.MinEligibleAmount)
                    return BaseResponse.Failure<LoanUpdateResponseDto>(
                        $"Requested amount is below the minimum of {check.MinEligibleAmount:N2}.");

                if (request.RequestedAmount.Value > check.MaxEligibleAmount)
                    return BaseResponse.Failure<LoanUpdateResponseDto>(
                        $"Requested amount exceeds your eligible maximum of {check.MaxEligibleAmount:N2}.");

                application.RequestedAmount = request.RequestedAmount.Value;
            }
        

            if (request.RequestedAmount.HasValue || request.TenorMonths.HasValue)
            {
                application.MonthlyRepayment = CalculateMonthlyRepayment(
                    application.RequestedAmount,
                    application.InterestRate,
                    application.TenorMonths);

                application.TotalRepayable = application.MonthlyRepayment * application.TenorMonths;
            }

            if (request.TenorMonths.HasValue)
            {
                var check = await _eligibilityRepository
                    .GetByIdAsync(application.EligibilityCheckId);

                decimal newRepayment = CalculateMonthlyRepayment(
                    application.RequestedAmount,
                    application.InterestRate,
                    request.TenorMonths.Value
                );

                if (newRepayment > check!.MaxMonthlyRepayment)
                    return BaseResponse.Failure<LoanUpdateResponseDto>(
                        $"The monthly repayment of {newRepayment:N2} for this tenor exceeds " +
                        $"your maximum affordable repayment of {check.MaxMonthlyRepayment:N2}. " +
                        $"Please increase the tenor or reduce the amount."
                    );

                application.TenorMonths = request.TenorMonths.Value;
            }

            if (!string.IsNullOrEmpty(request.LoanPurpose))
                application.LoanPurpose = request.LoanPurpose;

            

            application.UpdatedAt = DateTime.UtcNow;

            _loanApplicationRepository.Update(application);
            await _loanApplicationRepository.SaveChangesAsync();

            var response = _mapper.Map<LoanUpdateResponseDto>(application);
            response.IsSuccess = true;
            response.Message = "Application updated successfully.";
            return response;
        }

        public async Task<LoanDeleteResponseDto> CancelDraftApplication(Guid id)
        {
            var application = await _loanApplicationRepository.GetLoanApplicationWithDetailsAsync(id, CurrentApplicantId);
            if (application == null)
                return BaseResponse.Failure<LoanDeleteResponseDto>("Application not found or you do not have access to it.");

            if (application.Status != ApplicationStatus.Draft)
                return BaseResponse.Failure<LoanDeleteResponseDto>("Only draft applications can be cancelled.");

            try
            {
                // Soft Delete
                application.Status = ApplicationStatus.Cancelled;
                application.UpdatedAt = DateTime.UtcNow;
                _loanApplicationRepository.Update(application);

                await _loanApplicationRepository.SaveChangesAsync();

                return new LoanDeleteResponseDto 
                {
                    IsSuccess = true,
                    Message = "Loan Application Deleted Successfully!",
                    Id = application.Id,
                    ApplicationNumber = application.ApplicationNumber,
                    Status = application.Status.ToString(),
                    DeletedAt = application.UpdatedAt.Value
                };
            }
            catch (Exception)
            {
                return BaseResponse.Failure<LoanDeleteResponseDto>("An error occurred while cancelling the Loan application.");
            }
        }

        private static decimal CalculateMonthlyRepayment(
            decimal principal, decimal annualRatePercent, int tenorMonths)
        {
            if (annualRatePercent == 0)
                return Math.Round(principal / tenorMonths, 2);

            double r = (double)(annualRatePercent / 12 / 100);
            double n = tenorMonths;
            double p = (double)principal;

            // Standard annuity formula: PMT = P × r / (1 - (1 + r)^-n)
            double pmt = p * r / (1 - Math.Pow(1 + r, -n));

            return Math.Round((decimal)pmt, 2);
        }

        private async Task<string> GenerateApplicationNumber()
        {
            // Format: LQ-2026-000001
            var year = DateTime.UtcNow.Year;
            var count = await _loanApplicationRepository.GetTotalCountForYear(year);
            return $"LQ-{year}-{(count + 1).ToString("D6")}";
        }

    }
}
