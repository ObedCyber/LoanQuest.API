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
    public class ApplicantFinancialsRepository : GenericRepository<ApplicantFinancials>, IApplicantFinancialsRepository
    {
        public ApplicantFinancialsRepository(ApplicationDbContext context) : base(context) { }

        public async Task<ApplicantFinancials?> GetFinancialsByApplicantIdAsync(Guid applicantId)
        {
            return await _dbSet.FirstOrDefaultAsync(af => af.ApplicantId == applicantId);
        }
    }
}
