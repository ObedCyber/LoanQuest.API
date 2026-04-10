using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestDomain.Entities;
using LoanRequestDomain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LoanRequestInfrastructure.Persistence.Repositories
{
    public class LoanApplicationRepository : GenericRepository<LoanApplication>, ILoanApplicationRepository
    {
        public LoanApplicationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<int> GetTotalCountForYear(int year)
        {
            return await _dbSet
                .Where(x => x.CreatedAt.Year == year)
                .CountAsync();
        }

        public async Task<LoanApplication?> GetLoanApplicationByEligibilityCheckIdAsync(Guid eligibilityCheck)
        {
            return await _dbSet
                     .FirstOrDefaultAsync(x => x.EligibilityCheckId == eligibilityCheck);
        }

        public async Task<LoanApplication?> GetLoanApplicationWithDetailsAsync(Guid applicationId, Guid applicantId)
        {
            return await _dbSet
                .Include(x => x.LoanProduct)
                .FirstOrDefaultAsync(x => x.Id == applicationId && x.ApplicantId == applicantId);
        }

        public async Task<IEnumerable<LoanApplication>> GetAllLoanApplicationsForApplicantAsync(Guid applicantId)
        {
            return await _dbSet
                .Include(x => x.LoanProduct)
                .Where(x => x.ApplicantId == applicantId && x.Status != ApplicationStatus.Cancelled)
                .ToListAsync();
        }
    }
}
