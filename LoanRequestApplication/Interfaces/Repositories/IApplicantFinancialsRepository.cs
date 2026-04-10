using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestDomain.Entities;

namespace LoanRequestApplication.Interfaces.Repositories
{
    public interface IApplicantFinancialsRepository : IGenericRepository<ApplicantFinancials>
    {
        Task<ApplicantFinancials?> GetFinancialsByApplicantIdAsync(Guid applicantId);
    }
}
