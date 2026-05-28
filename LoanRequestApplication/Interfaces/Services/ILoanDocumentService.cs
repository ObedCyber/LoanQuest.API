using LoanRequestApplication.DTOs;

namespace LoanRequestApplication.Interfaces.Services
{
    public interface ILoanDocumentService
    {
        Task<LoanDocumentUploadResponse> ProcessLoanDocument(Guid id, LoanDocumentUploadRequest request);
        Task<LoanDocumentDeleteResponse> DeleteDocumentAsync(Guid loanApplicationId, Guid documentId);
    }
}
