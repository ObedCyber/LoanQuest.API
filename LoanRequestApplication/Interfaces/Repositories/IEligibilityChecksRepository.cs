using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestDomain.Entities;

namespace LoanRequestApplication.Interfaces.Repositories
{
    public interface IEligibilityChecksRepository : IGenericRepository<EligibilityChecks>
    {
        Task<EligibilityChecks?> GetEligibilityCheckByApplicantIdAsync(Guid applicantId);
        Task<IEnumerable<EligibilityChecks>> GetAllEligibilityChecksForByApplicantIdAsync(Guid applicantId);
    }
}
