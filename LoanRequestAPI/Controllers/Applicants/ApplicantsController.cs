using Azure;
using Azure.Core;
using LoanRequestApplication.DTOs;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestApplication.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoanRequestAPI.Controllers.Applicants
{
    [Authorize]
    [Route("api/[controller]/me")]
    [ApiController]
    public class ApplicantsController : ControllerBase
    {
        private readonly IBVN_NINService _bvn_ninService;
        private readonly IApplicantService _applicantService;
        private readonly IEmploymentService _employmentService;

        public ApplicantsController(IBVN_NINService bvn_ninService, IApplicantService applicantService, IEmploymentService employmentService)
        {
            _bvn_ninService = bvn_ninService;
            _applicantService = applicantService;
            _employmentService = employmentService;
        }

        [HttpPost("kyc/bvn")]
        public async Task<ActionResult<BvnKycResponse>> VerifyBVN([FromBody()] BvnKycRequest request)
        {
            var response = await _bvn_ninService.VerifyBVN(request);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ProfileRegistrationResponse>> RegisterProfile([FromBody()] ProfileRequest request)
        {
            var response = await _applicantService.RegisterApplicantAsync(request);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<ApplicantProfileDetails>> GetApplicantProfile()
        {
            var profile = await _applicantService.GetApplicantProfileAsync();
            if (profile == null)
            {
                return NotFound("Profile not found for the current user.");
            }
            return Ok(profile);
        }

        [HttpPut]
        public async Task<ActionResult<ProfileUpdateResponse>> UpdateProfile([FromBody] ProfileUpdateRequest request)
        {
            if (request == null) return BadRequest("Invalid update data.");

            var response = await _applicantService.UpdateApplicantAsync(request);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("employment")]
        public async Task<ActionResult<EmploymentResponse>> AddEmploymentDetails([FromBody] EmploymentRequest request)
        {
            if (request == null) return BadRequest("Invalid request data.");

            var response = await _employmentService.AddApplicantEmploymentDetails(request);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("financials")]
        public async Task<ActionResult<FinancialsResponse>> AddFinancialDetails([FromBody] FinancialsRequest request)
        {
            if (request == null) return BadRequest("Invalid request data.");
            var response = await _applicantService.AddApplicantFinancialDetails(request);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
