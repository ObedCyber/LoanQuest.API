using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.DTOs;

namespace LoanRequestApplication.Interfaces.Repositories
{
    public interface IEmploymentService 
    {
        Task<EmploymentResponse> AddApplicantEmploymentDetails(EmploymentRequest request);
    }
}
