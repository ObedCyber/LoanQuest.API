using LoanRequestApplication.DTOs;
using LoanRequestApplication.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanRequestAPI.Controllers.Loans
{
    /// <summary>
    /// Handles all loan-related operations including products, eligibility checks,
    /// applications, and document management.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LoansController : ControllerBase
    {
        private readonly ILoanProductService _loanProductService;
        private readonly IEligibilityEngine _eligibilityEngine;
        private readonly ILoanApplicationService _loanApplicationService;
        private readonly ILoanDocumentService _loanDocumentService;
        public LoansController(ILoanProductService loanProductService, IEligibilityEngine eligibilityEngine, ILoanApplicationService loanApplicationService, ILoanDocumentService loanDocumentService)
        {
            _loanProductService = loanProductService;
            _eligibilityEngine = eligibilityEngine;
            _loanApplicationService = loanApplicationService;
            _loanDocumentService = loanDocumentService;
        }
        /// <summary>
        /// Get all available loan products.
        /// </summary>
        /// <remarks>
        /// Returns the full list of active loan products the applicant can apply for.
        /// Each product includes its interest rate, minimum/maximum amount,
        /// allowed tenor range, and DSR cap.
        ///
        /// Use the returned <c>loanProductId</c> when running an eligibility check.
        ///
        /// **Example Response:**
        /// ```json
        /// {
        ///   "products": [
        ///     {
        ///       "id": "uuid",
        ///       "name": "Personal Loan",
        ///       "interestRatePercent": 18.0,
        ///       "minAmount": 50000,
        ///       "maxAmount": 5000000,
        ///       "minTenorMonths": 3,
        ///       "maxTenorMonths": 60
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <response code="200">List of active loan products returned successfully.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>

        [HttpGet("products")]
        public async Task<ActionResult<LoanProductFetchResponse>> GetLoanProducts()
        {
            var response = await _loanProductService.GetLoanProductsAsync();
            return Ok(response);
        }

        /// <summary>
        /// Run an eligibility check to find out how much you can borrow.
        /// </summary>
        /// <remarks>
        /// This is the **first step** before creating a loan application.
        /// It calculates your maximum eligible loan amount based on:
        /// - Your monthly gross salary (from your employment profile)
        /// - Your existing monthly obligations (from your financial profile)
        /// - The selected loan product's DSR and LTI rules
        /// - Your age relative to retirement age (60)
        ///
        /// **No credit bureau check is triggered here.** This is a soft pre-qualification only.
        ///
        /// The result includes:
        /// - Your eligible amount range (min and max)
        /// - A recommended amount
        /// - The monthly repayment for the recommended amount
        /// - A risk rating (Low / Medium / High / Decline)
        ///
        /// The eligibility check result is **valid for 24 hours**.
        /// Save the returned <c>eligibilityCheckId</c> — you will need it to create a loan application.
        ///
        /// **Prerequisites:** Your profile must have employment and financial details saved before running this check.
        ///
        /// </remarks>
        /// <param name="request">The loan product, desired amount, and tenor you want to check.</param>
        /// <response code="200">Eligibility check completed. Check <c>isEligible</c> in the response to see the result.</response>
        /// <response code="400">Missing profile data, inactive product, or tenor out of range.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>

        [HttpPost("eligibility-check")]
        public async Task<IActionResult> CalculateEligibility([FromBody] EligibilityRequestDto request)
        {
            var response = await _eligibilityEngine.Calculate(request);
            if (!response.IsSuccess) return Ok(new { response.IsSuccess, response.Message });
            return Ok(response);
        }

        /// <summary>
        /// Get the result of a previous eligibility check by its ID.
        /// </summary>
        /// <remarks>
        /// Retrieves a saved eligibility check result using the <c>eligibilityCheckId</c>
        /// returned when the check was originally run.
        ///
        /// Useful for reviewing a previous result before creating an application,
        /// or for confirming that a check is still within its 24-hour validity window.
        ///
        /// You can only retrieve checks that belong to your own account.
        /// </remarks>
        /// <param name="id">The unique ID of the eligibility check to retrieve.</param>
        /// <response code="200">Eligibility check found and returned successfully.</response>
        /// <response code="400">Check not found or does not belong to your account.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>

        [HttpGet("eligibility-check/{id:guid}")]
        public async Task<IActionResult> GetEligibilitycheck(Guid id)
        {
            var result = await _eligibilityEngine.GetEligibilityCheck(id);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok(result);
        }

        /// <summary>
        /// Get all eligibility checks you have ever run.
        /// </summary>
        /// <remarks>
        /// Returns every eligibility check associated with your account,
        /// ordered from most recent to oldest.
        ///
        /// This is useful for reviewing past pre-qualifications and checking
        /// which ones are still within their 24-hour validity window.
        ///
        /// Each result includes the check date, loan product, eligible range,
        /// and whether the check has expired.
        /// </remarks>
        /// <response code="200">
        /// Returns a list of your eligibility checks.
        /// If no checks exist, returns an empty list with <c>checkCount: 0</c>.
        /// </response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>

        [HttpGet("eligibility-check/all")]
        public async Task<IActionResult> GetAllEligibilityChecksForApplicant()
        {
            var result = await _eligibilityEngine.GetAllChecksByApplicantId();
            if (!result.Any())
            {
                return Ok(new { CheckCount = 0, Message = "No eligibility checks found for this user.", Data = result });
            }
            int checkCount = result.Count();
            return Ok(new { CheckCount = checkCount, Message = $"{checkCount} checks found!", Data = result });
        }
        /// <summary>
        /// Create a new loan application draft.
        /// </summary>
        /// <remarks>
        /// This is the **second step** after running a successful eligibility check.
        ///
        /// Creates a loan application in **Draft** status using the approved range
        /// from your eligibility check. The application is not submitted to loan
        /// officers yet — you can still edit it and upload documents.
        ///
        /// **Rules:**
        /// - The <c>eligibilityCheckId</c> must belong to your account
        /// - The check must not have expired (valid for 24 hours)
        /// - The <c>requestedAmount</c> must be within the eligible range from the check
        /// - The monthly repayment for the chosen amount and tenor must not exceed your DSR ceiling
        ///
        /// A document checklist is automatically generated based on the loan product's requirements.
        /// Upload all mandatory documents before submitting.
        ///
        /// **Example Request:**
        /// ```json
        /// {
        ///   "eligibilityCheckId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "requestedAmount": 1500000,
        ///   "tenorMonths": 24,
        ///   "loanPurpose": "Home renovation"
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Eligibility check reference, desired amount, tenor, and loan purpose.</param>
        /// <response code="200">Draft application created successfully. Returns the application number and document checklist.</response>
        /// <response code="400">Eligibility check expired, amount out of range, or missing profile data.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>

        [HttpPost("application")]
        public async Task<IActionResult> SubmitLoanApplication([FromBody] LoanApplicationRequestDto request)
        {
            var response = await _loanApplicationService.CreateLoanApplication(request);
            if (!response.IsSuccess) return BadRequest(response.Message);
            return Ok(response);
        }
        /// <summary>
        /// Get all loan applications for your account.
        /// </summary>
        /// <remarks>
        /// Returns a summary list of every loan application associated with your account,
        /// including Draft, Submitted, Under Review, Approved, and Rejected applications.
        ///
        /// Use <c>GET /api/loans/applications/{id}</c> to retrieve the full detail
        /// of a specific application including its document checklist.
        /// </remarks>
        /// <response code="200">
        /// Returns your applications with a total count.
        /// Returns <c>totalCount: 0</c> and an empty list if no applications exist.
        /// </response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>

        [HttpGet("applications")]
        public async Task<IActionResult> GetLoanApplicationsForApplicant()
        {
            var response = await _loanApplicationService.GetAllLoanApplicationsForApplicant();
            if (response.TotalCount == 0) return Ok(new { response.TotalCount, response.Applications });
            return Ok(response);
        }
        /// <summary>
        /// Get the full detail of a specific loan application.
        /// </summary>
        /// <remarks>
        /// Returns the complete application record including:
        /// - Loan amount, tenor, interest rate, and repayment figures
        /// - Current status and status history
        /// - The document checklist with upload status for each required document
        ///
        /// You can only retrieve applications that belong to your account.
        /// </remarks>
        /// <param name="id">The unique ID of the loan application.</param>
        /// <response code="200">Application found and returned with full details.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>
        /// <response code="404">Application not found or does not belong to your account.</response>
        [HttpGet("applications/{id:guid}")]
        public async Task<IActionResult> GetApplicationDetail(Guid id)
        {
            var response = await _loanApplicationService.GetApplicationDetailAsync(id);
            if (!response.IsSuccess) return NotFound(new { response.Message });

            return Ok(response);
        }
        /// <summary>
        /// Update a loan application that is still in Draft status.
        /// </summary>
        /// <remarks>
        /// Allows you to change the requested amount, tenor, or loan purpose
        /// on a Draft application before submitting it.
        ///
        /// **Rules:**
        /// - Only applications in **Draft** status can be updated
        /// - The new amount must still be within the eligible range from the original eligibility check
        /// - If the tenor changes, the new monthly repayment must not exceed your DSR ceiling
        /// - Repayment figures are automatically recalculated after any change
        ///
        /// All three fields are optional — only send the ones you want to change.
        ///
        /// **Example Request:**
        /// ```json
        /// {
        ///   "requestedAmount": 1200000,
        ///   "tenorMonths": 18,
        ///   "loanPurpose": "School fees"
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">The unique ID of the Draft application to update.</param>
        /// <param name="request">The fields you want to update. All fields are optional.</param>
        /// <response code="200">Application updated successfully. Returns the recalculated repayment figures.</response>
        /// <response code="400">Application is not in Draft status, amount out of range, or new repayment exceeds DSR limit.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>
        /// <response code="404">Application not found or does not belong to your account.</response>

        [HttpPut("applications/{id:guid}")]
        public async Task<IActionResult> UpdateLoanDraft(Guid id, [FromBody] LoanApplicationUpdateDto request)
        {
            var response = await _loanApplicationService.UpdateDraftApplication(id, request);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Cancel and delete a Draft loan application.
        /// </summary>
        /// <remarks>
        /// Permanently removes a loan application that is still in **Draft** status.
        ///
        /// **This action cannot be undone.**
        ///
        /// Only Draft applications can be cancelled. Applications that have already
        /// been submitted cannot be deleted.
        /// </remarks>
        /// <param name="id">The unique ID of the Draft application to cancel.</param>
        /// <response code="200">Application cancelled and removed successfully.</response>
        /// <response code="400">Application is not in Draft status and cannot be cancelled.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>
        /// <response code="404">Application not found or does not belong to your account.</response>
        [HttpDelete("applications/{id:guid}")]
        public async Task<IActionResult> CancelDraftApplication(Guid id)
        {
            var result = await _loanApplicationService.CancelDraftApplication(id);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Upload a document for a specific checklist item on a loan application.
        /// </summary>
        /// <remarks>
        /// Uploads a file to satisfy one item on the application's document checklist.
        /// The checklist is automatically created when a Draft application is first created
        /// and is based on the requirements of the selected loan product.
        ///
        /// **How to use:**
        /// Send the request as <c>multipart/form-data</c> with two fields:
        /// - <c>file</c> — the actual document file
        /// - <c>documentTypeCode</c> — the code of the checklist item you are satisfying
        ///   (e.g. <c>PAYSLIP</c>, <c>BANK_STMT</c>, <c>EMP_LETTER</c>, <c>VALID_ID</c>)
        ///
        /// **File Rules:**
        /// - Accepted formats: PDF, JPG, PNG
        /// - Maximum file size: 5MB
        /// - File name is sanitised on upload — a secure name is generated automatically
        ///
        /// **Re-uploading:** If a loan officer rejects a document, upload a new file
        /// for the same <c>documentTypeCode</c>. The old file is automatically replaced.
        ///
        /// **When can you upload?**
        /// Documents can only be uploaded while the application is in
        /// **Draft** or **More Info Required** status.
        ///
        /// The checklist item status will update to <c>Uploaded</c> after a successful upload.
        /// Once all mandatory documents are uploaded, the application is ready to submit.
        /// </remarks>
        /// <param name="id">The unique ID of the loan application.</param>
        /// <param name="request">
        /// Multipart form containing:
        /// - <c>file</c>: the document file (PDF, JPG, or PNG, max 5MB)
        /// - <c>documentTypeCode</c>: the checklist item code this file satisfies
        /// </param>
        /// <response code="200">Document uploaded successfully. Checklist item status updated to Uploaded.</response>
        /// <response code="400">
        /// Upload failed. Possible reasons:
        /// - Application is not in Draft or More Info Required status
        /// - Document type code is not on this application's checklist
        /// - File type is not allowed (must be PDF, JPG, or PNG)
        /// - File exceeds the size limit on checklist
        /// - No file was provided
        /// </response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>
        /// <response code="404">Application not found or does not belong to your account.</response>

        [HttpPost("applications/{id:guid}/documents")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument(Guid id, [FromForm] LoanDocumentUploadRequest request)
        {
            var result = await _loanDocumentService.ProcessLoanDocument(id, request);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Remove an uploaded document from a Draft application.
        /// </summary>
        /// <remarks>
        /// Permanently deletes an uploaded document from both the database
        /// and Azure Blob Storage. The checklist item is reset back to **Pending**.
        ///
        /// **Rules:**
        /// - Only works on applications in **Draft** status
        /// - The document must belong to this application
        ///
        /// Use this if you uploaded the wrong file and want to start over for that
        /// document type. After deleting, upload the correct file using
        /// <c>POST /api/loans/applications/{id}/documents</c>.
        ///
        /// **This action cannot be undone.** The file is permanently deleted from storage.
        /// </remarks>
        /// <param name="id">The unique ID of the loan application.</param>
        /// <param name="docId">The unique ID of the document to delete (returned when the document was uploaded).</param>
        /// <response code="200">Document deleted successfully. Checklist item reset to Pending.</response>
        /// <response code="400">Application is not in Draft status or document does not belong to this application.</response>
        /// <response code="401">Unauthorized. A valid JWT token is required.</response>
        /// <response code="404">Application or document not found.</response>

        [HttpDelete("applications/{id:guid}/documents/{docId:guid}")]
        public async Task<IActionResult> DeleteDocument(Guid id, Guid docId)
        {
            var result = await _loanDocumentService.DeleteDocumentAsync(id, docId);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }
    }
}
