using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestDomain.Entities;

namespace LoanRequestApplication.Interfaces.Repositories
{
    public interface ILoanApplicationRepository : IGenericRepository<LoanApplication>
    {
        Task<int> GetTotalCountForYear(int year);
        Task<IEnumerable<LoanApplication>> GetAllLoanApplicationsForApplicantAsync(Guid applicantId);
        Task<LoanApplication?> GetLoanApplicationWithDetailsAsync(Guid applicationId, Guid applicantId);
    }
}
