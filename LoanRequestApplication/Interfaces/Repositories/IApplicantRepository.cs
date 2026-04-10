using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestDomain.Entities;

namespace LoanRequestApplication.Interfaces.Repositories
{
    public interface IApplicantRepository : IGenericRepository<Applicant>
    {
        Task<bool> CheckApplicantWithUserIdExist(string userId);
        Task<Applicant?> GetApplicantByUserIdAsync(string userId);
    }
}
