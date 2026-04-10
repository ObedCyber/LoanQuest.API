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
    public class ApplicantEmploymentRepository : GenericRepository<ApplicantEmployment>, IApplicantEmploymentRepository
    {
        public ApplicantEmploymentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<ApplicantEmployment?> GetApplicantEmploymentByApplicantIdAsync (Guid applicantId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.ApplicantId == applicantId);
        }
    }
}
