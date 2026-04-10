using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.DTOs;
using LoanRequestDomain.Entities;

namespace LoanRequestApplication.Interfaces.Services
{
    public interface ILoanProductService
    {
        Task<LoanProductFetchResponse> GetLoanProductsAsync();
        Task<LoanProducts?> GetLoanProductByIdAsync(Guid id);
        Task<LoanProducts?> GetLoanProductByProductCode(string Code);
    }
}
