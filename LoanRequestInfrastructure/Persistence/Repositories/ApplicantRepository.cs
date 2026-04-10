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
    public class ApplicantRepository : GenericRepository<Applicant>, IApplicantRepository
    {
        public ApplicantRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> CheckApplicantWithUserIdExist(string userId)
        {
            return await _dbSet.AnyAsync(a => a.UserId == userId);
        }

        public async Task<Applicant?> GetApplicantByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(a => a.Financials)
                .Include(a => a.Employment)
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }
    }
}
