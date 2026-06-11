using AutoMapper;
using LoanRequestApplication.DTOs;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestApplication.Interfaces.Services;
using LoanRequestDomain.Entities;
using LoanRequestDomain.Enums;
using Microsoft.AspNetCore.Http;

namespace LoanRequestInfrastructure.Services.Loans
{
    public class EligibilityEngine : BaseService, IEligibilityEngine
    {
        private readonly ILoanProductService _loanProductService;
        private readonly IApplicantEmploymentRepository _employmentRepository;
        private readonly IApplicantRepository _applicantRepository;
        private readonly IApplicantFinancialsRepository _applicantFinancials;
        private readonly IEligibilityChecksRepository _eligibilityChecksRepository;
        private readonly IMapper _mapper;

        private const decimal NET_INCOME_FACTOR = 0.70m;

        private const int ELIGIBILITY_EXPIRY_HOURS = 24;

        public EligibilityEngine(
            ILoanProductService loanProductService,
            IApplicantEmploymentRepository employmentRepository,
            IApplicantRepository applicantRepository,
            IApplicantFinancialsRepository applicantfinancials,
            IEligibilityChecksRepository eligibilityChecksRepository,
            IMapper mapper,
            IHttpContextAccessor _httpContextAccessor
            ) : base(_httpContextAccessor)
        {
            _employmentRepository = employmentRepository;
            _loanProductService = loanProductService;
            _applicantRepository = applicantRepository;
            _applicantFinancials = applicantfinancials;
            _eligibilityChecksRepository = eligibilityChecksRepository;
            _mapper = mapper;
        }

        public async Task<EligibilityResultResponse> Calculate(EligibilityRequestDto request)
        {
            // if (applicant == null) return new EligibilityResultResponse { IsSuccess = false, Message = "Applicant profile does not exist.Please complete your profile." };
            if (CurrentApplicantId == Guid.Empty) return new EligibilityResultResponse { IsSuccess = false, Message = "Applicant profile does not exist.Please complete your profile." };
            var applicant = await _applicantRepository.GetApplicantByUserIdAsync(CurrentUserId!);
            // var applicantEmploymentDetails = await _employmentRepository.GetApplicantEmploymentByApplicantIdAsync(CurrentApplicantId);
            var applicantEmploymentDetails = applicant!.Employment;
            if (applicantEmploymentDetails == null) return new EligibilityResultResponse { IsSuccess = false, Message = "Employment details not found. Please complete your employment information." };

            //var applicantFinancialDetails = await _applicantFinancials.GetFinancialsByApplicantIdAsync(CurrentApplicantId);
            var applicantFinancialDetails = applicant.Financials;
            if (applicantFinancialDetails == null) return new EligibilityResultResponse { IsSuccess = false, Message = "Financial details not found. Please complete your financial information." };

            var applicantLoanProduct = await _loanProductService.GetLoanProductByIdAsync(request.LoanProductId);
            if (applicantLoanProduct == null) return new EligibilityResultResponse { IsSuccess = false, Message = "The selected loan product does not exist." };
            if (!applicantLoanProduct.IsActive) return new EligibilityResultResponse { IsSuccess = false, Message = "The selected loan product is currently unavailable." };

            if (request.RequestedTenorMonths > applicantLoanProduct.MaxTenorMonths ||
                request.RequestedTenorMonths < applicantLoanProduct.MinTenorMonths) return new EligibilityResultResponse { IsSuccess = false, Message = $"Tenor Must be between {applicantLoanProduct.MinTenorMonths} and {applicantLoanProduct.MaxTenorMonths} for thie Loan Product" };

            decimal MontlyNetIncome = applicantEmploymentDetails.MonthlyNetSalary == decimal.Zero ?
                applicantEmploymentDetails.MonthlyGrossSalary * NET_INCOME_FACTOR :
                applicantEmploymentDetails.MonthlyNetSalary;

            var result = RunCalculation(
                    MontlyNetIncome,
                    applicantEmploymentDetails.MonthlyGrossSalary,
                    applicantFinancialDetails.MonthlyObligations,
                    request.RequestedAmount,
                    request.RequestedTenorMonths,
                    applicant.DateOfBirth,
                    applicantLoanProduct
            );

            var eligibilitycheck = _mapper.Map<EligibilityChecks>(result);
            eligibilitycheck.ApplicantId = CurrentApplicantId;
            eligibilitycheck.LoanProductId = request.LoanProductId;
            eligibilitycheck.RequestedTenorMonths = request.RequestedTenorMonths;
            eligibilitycheck.MonthlyGrossSalary = applicantEmploymentDetails.MonthlyGrossSalary;
            eligibilitycheck.MonthlyObligations = applicantFinancialDetails.MonthlyObligations;
            eligibilitycheck.IpAddress = CurrentIpAddress;

            await _eligibilityChecksRepository.AddAsync(eligibilitycheck);
            bool saved = await _eligibilityChecksRepository.SaveChangesAsync();
            if (!saved) return new EligibilityResultResponse { IsSuccess = false, Message = "Failed to Save Eligibility Checks" };

            return new EligibilityResultResponse
            {
                IsSuccess = true,
                Message = result.IsEligible ? "Eligibility check completed successfully."
            : "You do not currently qualify for this loan product.",
                Data = result
            };
        }

        public async Task<EligibilityCheckDetailDto> GetEligibilityCheck(Guid id)
        {
            var check = await _eligibilityChecksRepository.GetByIdAsync(id);
            if (check == null) return new EligibilityCheckDetailDto { IsSuccess = false, Message = "Data not found." };
            var applicant = await _applicantRepository.GetApplicantByUserIdAsync(CurrentUserId!);
            if (applicant!.UserId != CurrentUserId) throw new UnauthorizedAccessException();
            var dto = _mapper.Map<EligibilityCheckDetailDto>(check);
            dto.IsSuccess = true;
            return dto;
        }

        public async Task<IEnumerable<EligibilityResponseDto?>> GetAllChecksByApplicantId()
        {
            if (CurrentApplicantId == Guid.Empty) throw new UnauthorizedAccessException("Invalid Applicant Id");
            var checks = await _eligibilityChecksRepository.GetAllEligibilityChecksForByApplicantIdAsync(CurrentApplicantId);
            var dto = _mapper.Map<IEnumerable<EligibilityResponseDto>>(checks);
            return dto;
        }

        internal static EligibilityResponseDto RunCalculation(
            decimal NetMonthlyIncome,
            decimal grossSalary,
            decimal monthlyObligations,
            decimal requestedAmount,
            int requestedTenor,
            DateTime dateOfBirth,
            LoanProducts product)
        {
            var response = new EligibilityResponseDto
            {
                IsEligible = true,
                EffectiveInterestRate = product.InterestRatePercent,
                DSRApplied = product.MaxDSRPercent,
                RequestedAmount = requestedAmount
            };

            decimal DisposableIncome = NetMonthlyIncome - monthlyObligations;

            if (DisposableIncome <= 0)
            {
                response.IsEligible = false;
                response.DisposableIncome = DisposableIncome;
                response.RejectionReasons.Add(RejectionReason.InsufficientDisposableIncome());

                return response;
            }

            int applicantAgeInMonths = GetAgeInMonths(dateOfBirth);
            int retirementAgeMonths = 60 * 12;
            int monthsToRetirement = retirementAgeMonths - applicantAgeInMonths;
            int allowedTenor = Math.Min(requestedTenor, monthsToRetirement);

            if (allowedTenor <= 0)
            {
                response.IsEligible = false;
                response.RejectionReasons.Add(
                    RejectionReason.TenorExceedsRetirementAge(monthsToRetirement)
                );
                return response;
            }

            int effectiveTenor = allowedTenor;

            if (effectiveTenor < requestedTenor)
            {
                response.RejectionReasons.Add(
                    RejectionReason.TenorExceedsRetirementAge(effectiveTenor)
                );
                response.IsEligible = false;
                return response;
            }

            decimal maxMonthlyRepayment = DisposableIncome * (product.MaxDSRPercent / 100);
            response.MaxMonthlyRepayment = Math.Round(maxMonthlyRepayment, 2);

            // Present Value of the loan = PMT × [(1 - (1 + r)^-n) / r]
            double monthlyRate = (double)(product.InterestRatePercent / 12 / 100);
            double n = effectiveTenor;
            double pmt = (double)maxMonthlyRepayment;

            decimal calculatedMaxAmount;

            if (monthlyRate == 0)
            {
                calculatedMaxAmount = (decimal)(pmt * n);
            }
            else
            {
                calculatedMaxAmount = (decimal)(
                    pmt / monthlyRate * (1 - Math.Pow(1 + monthlyRate, -n))
                );
            }

            // Loan-To-Income Ratio Cap 

            if (product.MaxLTIMultiplier > 0)
            {
                decimal ltiCap = grossSalary * 12 * product.MaxLTIMultiplier;
                calculatedMaxAmount = Math.Min(calculatedMaxAmount, ltiCap);
            }

            decimal finalMax = Math.Min(calculatedMaxAmount, product.MaxAmount);

            // Ensure the Loan is not more then Product Cap

            if (finalMax < product.MinAmount)
            {
                response.IsEligible = false;
                response.RejectionReasons.Add(
                    RejectionReason.AmountBelowMinimum(product.MinAmount)
                );

                // Still populate amounts so the caller knows how far off they are
                response.MaxEligibleAmount = 0;
                response.MinEligibleAmount = product.MinAmount;
                response.RecommendedAmount = 0;
                return response;
            }

            if (requestedAmount > finalMax)
            {
                response.IsEligible = false;
                response.RejectionReasons.Add(
                    RejectionReason.RequestedAmountExceedsEligible(
                        Math.Round(finalMax, 2)
                    )
                );
            }

            response.MaxEligibleAmount = Math.Round(finalMax, 2);
            response.MinEligibleAmount = product.MinAmount;

            // Recommended = the lesser of what they asked for and what they qualify for
            decimal recommendedAmount = Math.Min(requestedAmount, finalMax);
            response.RecommendedAmount = Math.Round(recommendedAmount, 2);

            decimal actualMonthlyRepayment = CalculateMonthlyRepayment(
            recommendedAmount, product.InterestRatePercent, effectiveTenor
        );

            response.MaxMonthlyRepayment = Math.Round(actualMonthlyRepayment, 2);
            response.TotalRepayable = Math.Round(actualMonthlyRepayment * effectiveTenor, 2);
            response.ExpiresAt = DateTime.UtcNow.AddHours(ELIGIBILITY_EXPIRY_HOURS);

            response.RiskRating = DetermineRiskRating(
                NetMonthlyIncome, monthlyObligations, product.MaxDSRPercent
            );

            return response;
        }

        private static int GetAgeInMonths(DateTime dateOfBirth)
        {
            // Precise month-level age — avoids integer year rounding errors
            DateTime today = DateTime.Today;

            int months = (today.Year - dateOfBirth.Year) * 12
                       + (today.Month - dateOfBirth.Month);

            // If we haven't reached the birth day this month yet, subtract one month
            if (today.Day < dateOfBirth.Day)
                months--;

            return months;
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

        private static string DetermineRiskRating(
            decimal netMonthlyIncome,
            decimal monthlyObligations,
            decimal maxAllowedDsr)
        {
            // 1. Immediate Decline for zero or negative income
            if (netMonthlyIncome <= 0) return RiskRating.Decline.ToString();

            // 2. Calculate current Debt-to-Income (DTI) as a percentage
            decimal currentDsr = (monthlyObligations / netMonthlyIncome) * 100;

            // 3. Determine Rating based on proximity to the DSR limit
            // We use "Relative" thresholds so it works for all products
            return currentDsr switch
            {
                var dsr when dsr > maxAllowedDsr => RiskRating.Decline.ToString(),
                var dsr when dsr >= (maxAllowedDsr * 0.8m) => RiskRating.High.ToString(),   // Near the limit
                var dsr when dsr >= (maxAllowedDsr * 0.4m) => RiskRating.Medium.ToString(), // Moderate debt
                _ => RiskRating.Low.ToString()                                              // Plenty of room
            };
        }


    }
}
