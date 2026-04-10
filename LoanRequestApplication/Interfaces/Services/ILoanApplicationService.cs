using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.DTOs;

namespace LoanRequestApplication.Interfaces.Services
{
    public interface ILoanApplicationService
    {
        Task<LoanApplicationResponseDto> CreateLoanApplication(LoanApplicationRequestDto request);
        Task<LoanApplicationListResponseDto> GetAllLoanApplicationsForApplicant();
        Task<LoanApplicationDetailResponseDto> GetApplicationDetailAsync(Guid id);
        Task<LoanUpdateResponseDto> UpdateDraftApplication(Guid id, LoanApplicationUpdateDto request);
        Task<LoanDeleteResponseDto> CancelDraftApplication(Guid id);
    }
}
