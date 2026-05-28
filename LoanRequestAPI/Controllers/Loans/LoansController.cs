using LoanRequestApplication.DTOs;
using LoanRequestApplication.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanRequestAPI.Controllers.Loans
{
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

        [HttpGet("products")]
        public async Task<ActionResult<LoanProductFetchResponse>> GetLoanProducts()
        {
            var response = await _loanProductService.GetLoanProductsAsync();
            return Ok(response);
        }

        [HttpPost("eligibility-check")]
        public async Task<IActionResult> CalculateEligibility([FromBody] EligibilityRequestDto request)
        {
            var response = await _eligibilityEngine.Calculate(request);
            if (!response.IsSuccess) return Ok(new { response.IsSuccess, response.Message });
            return Ok(response);
        }

        [HttpGet("eligibility-check/{id:guid}")]
        public async Task<IActionResult> GetEligibilitycheck(Guid id)
        {
            var result = await _eligibilityEngine.GetEligibilityCheck(id);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok(result);
        }


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

        [HttpPost("application")]
        public async Task<IActionResult> SubmitLoanApplication([FromBody] LoanApplicationRequestDto request)
        {
            var response = await _loanApplicationService.CreateLoanApplication(request);
            if (!response.IsSuccess) return BadRequest(response.Message);
            return Ok(response);
        }

        [HttpGet("applications")]
        public async Task<IActionResult> GetLoanApplicationsForApplicant()
        {
            var response = await _loanApplicationService.GetAllLoanApplicationsForApplicant();
            if (response.TotalCount == 0) return Ok(new { response.TotalCount, response.Applications });
            return Ok(response);
        }

        [HttpGet("applications/{id:guid}")]
        public async Task<IActionResult> GetApplicationDetail(Guid id)
        {
            var response = await _loanApplicationService.GetApplicationDetailAsync(id);
            if (!response.IsSuccess) return NotFound(new { response.Message });

            return Ok(response);
        }

        [HttpPut("applications/{id:guid}")]
        public async Task<IActionResult> UpdateLoanDraft(Guid id, [FromBody] LoanApplicationUpdateDto request)
        {
            var response = await _loanApplicationService.UpdateDraftApplication(id, request);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("applications/{id:guid}")]
        public async Task<IActionResult> CancelDraftApplication(Guid id)
        {
            var result = await _loanApplicationService.CancelDraftApplication(id);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("applications/{id:guid}/documents")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument(Guid id, [FromForm] LoanDocumentUploadRequest request)
        {
            var result = await _loanDocumentService.ProcessLoanDocument(id, request);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("applications/{id:guid}/documents/{docId:guid}")]
        public async Task<IActionResult> DeleteDocument(Guid id, Guid docId)
        {
            var result = await _loanDocumentService.DeleteDocumentAsync(id, docId);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }
    }
}
