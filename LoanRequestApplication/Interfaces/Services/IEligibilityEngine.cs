using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.DTOs;

namespace LoanRequestApplication.Interfaces.Services
{
    public interface IEligibilityEngine
    {
        public Task<EligibilityResultResponse> Calculate(EligibilityRequestDto request);
        public Task<EligibilityCheckDetailDto> GetEligibilityCheck(Guid id);
        public Task<IEnumerable<EligibilityResponseDto?>> GetAllChecksByApplicantId();
    }
}
