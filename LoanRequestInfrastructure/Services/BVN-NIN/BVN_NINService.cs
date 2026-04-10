using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.DTOs;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestApplication.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LoanRequestInfrastructure.Services.BVN_NIN
{
    public class BVN_NINService : BaseService, IBVN_NINService
    {
        private readonly string _FlutterwavesecretKey;
        private readonly IConfiguration _config;
        private readonly IApplicantRepository _Applicantrepository;
        private readonly ILogger<BVN_NINService> _logger;
        private readonly HttpClient _httpClient;
        

        public BVN_NINService(IConfiguration config, ILogger<BVN_NINService> logger, HttpClient httpClient, IHttpContextAccessor httpContextAccess, IApplicantRepository repository) : base(httpContextAccess)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
            _Applicantrepository = repository;
            _FlutterwavesecretKey = _config["FlutterWave:Key"] ?? throw new InvalidOperationException("Flutterwave secret Key not configured");
        }

        public async Task<BvnKycResponse> VerifyBVN(BvnKycRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Bvn))
                return BaseResponse.Failure<BvnKycResponse>("BVN cannot be empty.");

            // CurrentUserId is available from your BaseService
            var applicant = await _Applicantrepository.GetApplicantByUserIdAsync(CurrentUserId);

            if (applicant == null)
                return BaseResponse.Failure<BvnKycResponse>("Applicant profile not found. Please create a profile first.");

            if (!string.IsNullOrEmpty(applicant.BVN))
                return BaseResponse.Failure<BvnKycResponse>("BVN has already been validated for this account.");

            var bvnRequest = new BVNValidationRequest
            {
                Bvn = request.Bvn,
                Firstname = applicant.FirstName,
                Lastname = applicant.LastName
            };


            var response = await ExecuteFlutterwaveBvnCall(bvnRequest);

            if (response.Status == "success")
            {
                applicant.BVN = request.Bvn;
                _Applicantrepository.Update(applicant);
                await _Applicantrepository.SaveChangesAsync();

                return BaseResponse.Success<BvnKycResponse>("BVN Verification successful.");
            }

            return BaseResponse.Failure<BvnKycResponse>($"Verification failed: {response.Message}");
        }

        internal async Task<BvnVerificationResponse> ExecuteFlutterwaveBvnCall(BVNValidationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Request cannot be null");
            }
            if (string.IsNullOrWhiteSpace(request.Bvn) || request.Bvn.Length != 11)
            {
                return new BvnVerificationResponse
                {
                    Status = "error",
                    Message = "Invalid BVN length. Must be 11 digits.",
                    Data = null
                };
            }
            await Task.Delay(500);

            return new BvnVerificationResponse
            {
                Status = "success",
                Message = "Bvn verification initiated",
                Data = new BvnData
                {
                    Url = $"https://mock-consent.company.com/cms/BvnConsent?session={Guid.NewGuid()}",
                    Reference = $"MOCK-{Guid.NewGuid().ToString("N").ToUpper().Substring(0, 12)}"
                }
            };
        }
    }
}
