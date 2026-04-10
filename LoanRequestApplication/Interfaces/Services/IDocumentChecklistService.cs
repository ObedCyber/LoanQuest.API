using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestDomain.Entities;

namespace LoanRequestApplication.Interfaces.Services
{
    public interface IDocumentChecklistService
    {
        Task<bool> CreateChecklistForApplication(Guid applicationId, Guid productId);
        Task<IEnumerable<ApplicationDocumentChecklist>?> GetChecklistForApplication(Guid applicationId);
        Task<bool> CheckApplicantDocumentSnapshot(Guid applicationId);
    }
}
