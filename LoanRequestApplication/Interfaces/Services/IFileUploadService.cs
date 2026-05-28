using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.DTOs;
using Microsoft.AspNetCore.Http;

namespace LoanRequestApplication.Interfaces.Services
{
    public interface IFileUploadService
    {
       Task<BlobUploadResult> UploadDocumentAsync(IFormFile file, Guid loanApplicationId, string docTypeCode);
        Task DeleteDocumentAsync(string blobPath);
    }
}
