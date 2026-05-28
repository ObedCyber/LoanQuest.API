using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.DTOs;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestApplication.Interfaces.Services;
using LoanRequestDomain.Entities;
using LoanRequestDomain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LoanRequestInfrastructure.Services.Loans
{
    public class LoanDocumentService : ILoanDocumentService
    {
        public readonly IFileUploadService _uploadService;
        private readonly IGenericRepository<LoanDocument> _loanDocumentRepository;
        private readonly IGenericRepository<ApplicationDocumentChecklist> _checklistRepo;

        public LoanDocumentService(
            IFileUploadService uploadService,
            IGenericRepository<LoanDocument> loanDocumentRepository,
            IGenericRepository<ApplicationDocumentChecklist> checklistRepo
            )
        {
            _uploadService = uploadService;
            _loanDocumentRepository = loanDocumentRepository;
            _checklistRepo = checklistRepo;
        }

        public async Task<LoanDocumentUploadResponse> ProcessLoanDocument (Guid id, LoanDocumentUploadRequest request)
        {
            if (id == Guid.Empty) return BaseResponse.Failure<LoanDocumentUploadResponse>(message: "Application ID cannot be empty");
            var documentChecklist = await _checklistRepo.Query().Where(a => a.LoanApplicationId == id && a.DocumentTypeCode == request.DocumentTypeCode).FirstOrDefaultAsync();
            if (documentChecklist == null) return BaseResponse.Failure<LoanDocumentUploadResponse>(message: "Invalid Application ID or Document Type Code");
            if (documentChecklist.Status == ChecklistItemStatus.Verified) return BaseResponse.Failure<LoanDocumentUploadResponse>(message: "Cannot Re-upload Verified Document");
            if (documentChecklist.Status == ChecklistItemStatus.Uploaded || documentChecklist.Status == ChecklistItemStatus.Rejected)
            {
                var uploadedDocument = await _loanDocumentRepository.Query().Where(d => d.Id == documentChecklist.LoanDocumentId).FirstOrDefaultAsync();
                if(uploadedDocument != null) await _uploadService.DeleteDocumentAsync(uploadedDocument.StoragePath);
            }

            var allowedExtensions = documentChecklist.AllowedFileTypes
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => "." + ext.Trim().ToLower())
            .ToList();

            var uploadedExtension = Path.GetExtension(request.File.FileName).ToLower();

            if (!allowedExtensions.Contains(uploadedExtension)) return BaseResponse.Failure<LoanDocumentUploadResponse>(message: $"Invalid file extension, only {documentChecklist.AllowedFileTypes} are allowed for {documentChecklist.DocumentTypeCode} Document Types");
            long maxFileSizeBytes = (long)documentChecklist.MaxFileSizeMb * 1024 * 1024;

            if (request.File.Length > maxFileSizeBytes) return BaseResponse.Failure<LoanDocumentUploadResponse>(message: $"File size exceeds the limit. '{documentChecklist.DocumentTypeName}' cannot be larger than {documentChecklist.MaxFileSizeMb}MB.");

            var AzureUploadResult = await _uploadService.UploadDocumentAsync(request.File, id, documentChecklist.DocumentTypeCode);
            if (!AzureUploadResult.IsSuccess) return BaseResponse.Failure<LoanDocumentUploadResponse>(message: AzureUploadResult.ErrorMessage!);

            var loanDoc = new LoanDocument
            {
                LoanApplicationId = id,
                ApplicantId = documentChecklist.LoanApplication.ApplicantId,
                DocumentTypeCode = documentChecklist.DocumentTypeCode,
                DocumentTypeName = documentChecklist.DocumentTypeName,
                FileName = request.FileName,
                StoragePath = AzureUploadResult.StoragePath!,
                FileSize = request.File.Length,
                ContentType = request.File.ContentType,
                Status = DocumentStatus.Pending,
                UploadedAt = DateTime.UtcNow,
                LoanApplication = documentChecklist.LoanApplication,
                Applicant = documentChecklist.LoanApplication.Applicant
            };

            try
            {
                await _loanDocumentRepository.AddAsync(loanDoc);

                documentChecklist.Status = ChecklistItemStatus.Uploaded;
                documentChecklist.LoanDocumentId = loanDoc.Id;
                documentChecklist.UpdatedAt = DateTime.UtcNow;

                await _loanDocumentRepository.SaveChangesAsync();
            }
            catch (Exception)
            {
                return BaseResponse.Failure<LoanDocumentUploadResponse>(message: "Error Saving File Details to Database");
            }

            return BaseResponse.Success<LoanDocumentUploadResponse>(message: "Loan Uploaded Successfully!");
        }
    }
}
