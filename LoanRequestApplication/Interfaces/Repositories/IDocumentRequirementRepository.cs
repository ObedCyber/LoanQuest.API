using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestDomain.Entities;

namespace LoanRequestApplication.Interfaces.Repositories
{
    public interface IDocumentRequirementRepository : IGenericRepository<DocumentRequirement>
    {
        Task<IEnumerable<DocumentRequirement>> GetDocumentRequirementsByLoanProductIdAsync(Guid loanProductId);
    }
}
