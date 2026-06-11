using LoanRequestApplication.DTOs;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestApplication.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanRequestAPI.Controllers.Applicants
{
    /// <summary>
    /// Manages the applicant's personal profile, employment details,
    /// financial information, and identity verification (KYC).
    /// </summary>
    /// <remarks>
    /// All endpoints are scoped to the currently authenticated user.
    /// You cannot access or modify another applicant's profile.
    ///
    /// **Recommended setup order before running an eligibility check:**
    /// 1. POST /api/applicants/me — Create your profile
    /// 2. POST /api/applicants/me/employment — Add employment details
    /// 3. POST /api/applicants/me/financials — Add financial obligations
    /// 4. POST /api/applicants/me/kyc/bvn — Verify your BVN
    /// </remarks>
    [Authorize]
    [Route("api/[controller]/me")]
    [ApiController]
    public class ApplicantsController : ControllerBase
    {
        private readonly IBVN_NINService _bvn_ninService;
        private readonly IApplicantService _applicantService;
        private readonly IEmploymentService _employmentService;

        public ApplicantsController(
            IBVN_NINService bvn_ninService,
            IApplicantService applicantService,
            IEmploymentService employmentService)
        {
            _bvn_ninService = bvn_ninService;
            _applicantService = applicantService;
            _employmentService = employmentService;
        }

        // ── PROFILE ───────────────────────────────────────────────────────────

        /// <summary>
        /// Create your applicant profile.
        /// </summary>
        /// <remarks>
        /// This is the **first step** after registering and logging in.
        /// Creates your personal profile linked to your user account.
        ///
        /// Your profile stores your personal information such as your name,
        /// date of birth, residential address, and state of origin.
        /// This information is used during the loan eligibility check and
        /// for KYC verification.
        ///
        /// **You can only have one profile per account.**
        /// If a profile already exists, use <c>PUT /api/applicants/me</c> to update it.
        ///
        /// **Example Request:**
        /// ```json
        /// {
        ///   "firstName": "John",
        ///   "lastName": "Doe",
        ///   "dateOfBirth": "1988-05-15",
        ///   "gender": "Male",
        ///   "maritalStatus": "Married",
        ///   "residentialAddress": "12 Admiralty Way, Lekki Phase 1",
        ///   "residentialState": "Lagos",
        ///   "stateOfOrigin": "Anambra"
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Your personal information to register as an applicant profile.</param>
        /// <response code="200">Profile created successfully. Returns the new profile with a generated applicant ID.</response>
        /// <response code="400">Profile already exists for this account, or required fields are missing.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ProfileRegistrationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProfileRegistrationResponse>> RegisterProfile(
            [FromBody] ProfileRequest request)
        {
            var response = await _applicantService.RegisterApplicantAsync(request);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Get your current applicant profile.
        /// </summary>
        /// <remarks>
        /// Returns your full applicant profile including:
        /// - Personal information (name, DOB, address)
        /// - KYC verification status for each check (Email, Phone, BVN, NIN)
        /// - Employment details (if added)
        /// - Financial obligations (if added)
        /// - Profile completeness percentage
        ///
        /// Use this endpoint to check what information is still missing
        /// before running an eligibility check or submitting an application.
        ///
        /// The <c>profileCompleteness</c> field shows your progress as a percentage.
        /// A minimum of **80%** is required before running an eligibility check.
        /// **100%** is required before submitting a loan application.
        /// </remarks>
        /// <response code="200">Profile found and returned successfully.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>
        /// <response code="404">No profile found for this account. Use POST /api/applicants/me to create one.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApplicantProfileDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicantProfileDetails>> GetApplicantProfile()
        {
            var profile = await _applicantService.GetApplicantProfileAsync();
            if (profile == null)
                return NotFound("Profile not found for the current user.");
            return Ok(profile);
        }

        /// <summary>
        /// Update your applicant profile.
        /// </summary>
        /// <remarks>
        /// Updates your existing personal profile. Only the fields you include
        /// in the request body will be updated — fields you leave out remain unchanged.
        ///
        /// **Commonly updated fields:**
        /// - Residential address (if you have moved)
        /// - Marital status
        /// - Phone number
        ///
        /// **Note:** Your date of birth cannot be changed after BVN verification
        /// because it is cross-checked against the BVN record.
        ///
        /// After a successful update, your profile completeness percentage
        /// is automatically recalculated and returned.
        /// </remarks>
        /// <param name="request">The profile fields you want to update. Only include fields you wish to change.</param>
        /// <response code="200">Profile updated successfully. Returns the updated profile with recalculated completeness.</response>
        /// <response code="400">Invalid update data or update violates a business rule.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>
        /// <response code="404">No profile found. Create one first using POST /api/applicants/me.</response>
        [HttpPut]
        [ProducesResponseType(typeof(ProfileUpdateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProfileUpdateResponse>> UpdateProfile(
            [FromBody] ProfileUpdateRequest request)
        {
            if (request == null) return BadRequest("Invalid update data.");
            var response = await _applicantService.UpdateApplicantAsync(request);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        // ── EMPLOYMENT ────────────────────────────────────────────────────────

        /// <summary>
        /// Add your employment details.
        /// </summary>
        /// <remarks>
        /// Saves your current employment information to your profile.
        /// This is **required** before running an eligibility check because
        /// your monthly gross salary is the primary input to the eligibility engine.
        ///
        /// **What this information is used for:**
        /// - <c>monthlyGrossSalary</c> — used to calculate your net income, DSR cap, and maximum eligible loan amount
        /// - <c>employmentStartDate</c> — used to determine employment stability
        /// - <c>employerName</c> — cross-referenced during document verification
        ///
        /// **Example Request:**
        /// ```json
        /// {
        ///   "employmentType": "Employed",
        ///   "employerName": "GTBank Plc",
        ///   "jobTitle": "Software Engineer",
        ///   "monthlyGrossSalary": 500000,
        ///   "employmentStartDate": "2019-01-15",
        ///   "salaryAccountBank": "GTBank",
        ///   "salaryAccountNumber": "0123456789"
        /// }
        /// ```
        ///
        /// **Supported Employment Types:** Employed, Self-Employed, Contract, Retired
        /// </remarks>
        /// <param name="request">Your current employer and salary details.</param>
        /// <response code="200">Employment details saved successfully. Profile completeness is recalculated.</response>
        /// <response code="400">Invalid request data or required salary fields are missing.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>
        [HttpPost("employment")]
        [ProducesResponseType(typeof(EmploymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<EmploymentResponse>> AddEmploymentDetails(
            [FromBody] EmploymentRequest request)
        {
            if (request == null) return BadRequest("Invalid request data.");
            var response = await _employmentService.AddApplicantEmploymentDetails(request);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        // ── FINANCIALS ────────────────────────────────────────────────────────

        /// <summary>
        /// Add your monthly financial obligations.
        /// </summary>
        /// <remarks>
        /// Saves your existing monthly financial commitments to your profile.
        /// This is **required** before running an eligibility check.
        ///
        /// **Why this matters:**
        /// The eligibility engine deducts your existing obligations from your
        /// net income to calculate your true disposable income. This determines
        /// the maximum monthly repayment you can afford and directly affects
        /// how much you can borrow.
        ///
        /// > Disposable Income = Net Monthly Income − Monthly Obligations
        ///
        /// **If you have no existing loans or commitments**, submit this endpoint
        /// with <c>monthlyObligations: 0</c>. The record must exist for the
        /// eligibility check to proceed.
        ///
        /// **Example Request:**
        /// ```json
        /// {
        ///   "monthlyObligations": 80000,
        ///   "otherMonthlyIncome": 50000
        /// }
        /// ```
        ///
        /// - <c>monthlyObligations</c> — total of all existing loan repayments per month
        /// - <c>otherMonthlyIncome</c> — any additional income (rent, investments, freelance). Optional.
        /// </remarks>
        /// <param name="request">Your existing monthly financial obligations and any additional income.</param>
        /// <response code="200">Financial details saved successfully. Profile completeness is recalculated.</response>
        /// <response code="400">Invalid request data or required fields are missing.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>
        [HttpPost("financials")]
        [ProducesResponseType(typeof(FinancialsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<FinancialsResponse>> AddFinancialDetails(
            [FromBody] FinancialsRequest request)
        {
            if (request == null) return BadRequest("Invalid request data.");
            var response = await _applicantService.AddApplicantFinancialDetails(request);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        // ── KYC ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Verify your Bank Verification Number (BVN).
        /// </summary>
        /// <remarks>
        /// Submits your BVN to a licensed identity verification provider (NIBSS via gateway)
        /// for cross-checking against your registered profile details.
        ///
        /// **How it works:**
        /// 1. Your BVN and date of birth are sent to the verification provider
        /// 2. The provider returns the name and DOB linked to that BVN
        /// 3. The returned details are cross-checked against your profile
        /// 4. If they match, your KYC status updates to **Verified**
        /// 5. If they do not match, the attempt is logged and you can retry
        ///
        /// **Important:**
        /// - BVN verification is required before your profile can reach 100% completeness
        /// - A verified BVN is required before submitting a loan application
        /// - Your raw BVN is never stored — only the verification result is saved
        ///
        /// **Example Request:**
        /// ```json
        /// {
        ///   "bvn": "12345678901",
        ///   "dateOfBirth": "1988-05-15"
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Your 11-digit BVN and date of birth for cross-checking.</param>
        /// <response code="200">
        /// BVN verification processed. Check <c>isSuccess</c> in the response:
        /// - <c>true</c> — BVN verified, KYC status updated
        /// - <c>false</c> — Verification failed (name or DOB mismatch)
        /// </response>
        /// <response code="400">Invalid BVN format, missing date of birth, or no profile found.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>
        [HttpPost("kyc/bvn")]
        [ProducesResponseType(typeof(BvnKycResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BvnKycResponse>> VerifyBVN(
            [FromBody] BvnKycRequest request)
        {
            var response = await _bvn_ninService.VerifyBVN(request);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }
    }
}