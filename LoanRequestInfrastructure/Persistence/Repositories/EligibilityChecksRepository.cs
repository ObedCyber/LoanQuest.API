using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanRequestInfrastructure.Persistence.Repositories
{
    public class EligibilityChecksRepository : GenericRepository<EligibilityChecks>, IEligibilityChecksRepository
    {
        public EligibilityChecksRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<EligibilityChecks?> GetEligibilityCheckByApplicantIdAsync(Guid applicantId)
        {
            return await _dbSet.FirstOrDefaultAsync(ec => ec.ApplicantId == applicantId);
        }


        public async Task<IEnumerable<EligibilityChecks>> GetAllEligibilityChecksForByApplicantIdAsync(Guid applicantId)
        {
            return await _dbSet.Where(ec => ec.ApplicantId == applicantId).ToListAsync();
        }
    }
}
