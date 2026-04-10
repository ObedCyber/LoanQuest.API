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
    public class DocumentRequirementRepository : GenericRepository<DocumentRequirement>, IDocumentRequirementRepository
    {
        public DocumentRequirementRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DocumentRequirement>> GetDocumentRequirementsByLoanProductIdAsync(Guid loanProductId)
        {
            return await _dbSet.Where(dr => dr.LoanProductId == loanProductId).ToListAsync();
        }
    }
}
