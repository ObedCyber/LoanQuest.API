using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.DTOs;

namespace LoanRequestApplication.Interfaces.Services
{
    public interface ILoanDocumentService
    {
        Task<LoanDocumentUploadResponse> ProcessLoanDocument(Guid id, LoanDocumentUploadRequest request);
    }
}
