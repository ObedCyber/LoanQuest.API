using LoanRequestApplication.DTOs;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestApplication.Interfaces.Services;
using LoanRequestDomain.Entities;
using LoanRequestDomain.Enums;
using Microsoft.AspNetCore.Http;
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

        //public async Task<LoanDocumentUploadResponse> ProcessLoanDocument(Guid id, LoanDocumentUploadRequest request)
        //{
        //    if (id == Guid.Empty) return BaseResponse.Failure<LoanDocumentUploadResponse>(message: "Application ID cannot be empty");

        //    var documentChecklist = await _checklistRepo.Query()
        //        .Include(a => a.LoanApplication)
        //        .ThenInclude(l => l.Applicant)
        //        .Where(a => a.LoanApplicationId == id && a.DocumentTypeCode == request.DocumentTypeCode)
        //        .FirstOrDefaultAsync();

        //    if (documentChecklist == null) return BaseResponse.Failure<LoanDocumentUploadResponse>(message: "Invalid Application ID or Document Type Code");
        //    if (documentChecklist.Status == ChecklistItemStatus.Verified) return BaseResponse.Failure<LoanDocumentUploadResponse>(message: "Cannot Re-upload Verified Document");
        //    if (documentChecklist.Status == ChecklistItemStatus.Uploaded || documentChecklist.Status == ChecklistItemStatus.Rejected)
        //    {
        //        var uploadedDocument = await _loanDocumentRepository.Query().Where(d => d.Id == documentChecklist.LoanDocumentId).FirstOrDefaultAsync();
        //        if (uploadedDocument != null)
        //        {
        //            await _uploadService.DeleteDocumentAsync(uploadedDocument.StoragePath);
        //            uploadedDocument.IsDeleted = true;
        //            _loanDocumentRepository.Update(uploadedDocument);
        //            await _loanDocumentRepository.SaveChangesAsync();
        //        }
        //    }

        //    var allowedExtensions = documentChecklist.AllowedFileTypes
        //    .Split([','], StringSplitOptions.RemoveEmptyEntries)
        //    .Select(ext => "." + ext.Trim().ToLower())
        //    .ToList();

        //    var uploadedExtension = Path.GetExtension(request.File.FileName).ToLower();

        //    if (!allowedExtensions.Contains(uploadedExtension)) return BaseResponse.Failure<LoanDocumentUploadResponse>(message: $"Invalid file extension, only {documentChecklist.AllowedFileTypes} are allowed for {documentChecklist.DocumentTypeCode} Document Types");
        //    long maxFileSizeBytes = (long)documentChecklist.MaxFileSizeMb * 1024 * 1024;

        //    if (request.File.Length > maxFileSizeBytes) return BaseResponse.Failure<LoanDocumentUploadResponse>(message: $"File size exceeds the limit. '{documentChecklist.DocumentTypeName}' cannot be larger than {documentChecklist.MaxFileSizeMb}MB.");

        //    var AzureUploadResult = await _uploadService.UploadDocumentAsync(request.File, id, documentChecklist.DocumentTypeCode);
        //    if (!AzureUploadResult.IsSuccess) return BaseResponse.Failure<LoanDocumentUploadResponse>(message: AzureUploadResult.ErrorMessage!);

        //    var loanDoc = new LoanDocument
        //    {
        //        LoanApplicationId = id,
        //        ApplicantId = documentChecklist.LoanApplication.ApplicantId,
        //        DocumentTypeCode = documentChecklist.DocumentTypeCode,
        //        DocumentTypeName = documentChecklist.DocumentTypeName,
        //        FileName = request.FileName,
        //        StoragePath = AzureUploadResult.StoragePath!,
        //        FileSize = request.File.Length,
        //        ContentType = request.File.ContentType,
        //        Status = DocumentStatus.Pending,
        //        UploadedAt = DateTime.UtcNow,
        //        LoanApplication = documentChecklist.LoanApplication,
        //        Applicant = documentChecklist.LoanApplication.Applicant
        //    };

        //    try
        //    {              
        //        await _loanDocumentRepository.AddAsync(loanDoc);

        //        documentChecklist.Status = ChecklistItemStatus.Uploaded;
        //        documentChecklist.LoanDocumentId = loanDoc.Id;
        //        documentChecklist.UpdatedAt = DateTime.UtcNow;

        //        await _loanDocumentRepository.SaveChangesAsync();
        //    }
        //    catch (Exception)
        //    {
        //        await _uploadService.DeleteDocumentAsync(AzureUploadResult.StoragePath!);
        //        return BaseResponse.Failure<LoanDocumentUploadResponse>(message: "Error Saving File Details to Database");
        //    }
        //    var response = new LoanDocumentUploadResponse
        //    {
        //        DocumentId = loanDoc.Id,
        //        DocumentTypeCode = loanDoc.DocumentTypeCode,
        //        DocumentTypeName = loanDoc.DocumentTypeName,
        //        FileName = loanDoc.FileName,
        //        Status = loanDoc.Status.ToString(), 
        //        UploadedAt = loanDoc.UploadedAt,
        //        IsSuccess = true,
        //        Message = "Document Uploaded Successfully!"
        //    };

        //    return response;
        //}

        public async Task<LoanDocumentUploadResponse> ProcessLoanDocument(Guid loanApplicationId, LoanDocumentUploadRequest request)
        {
            if (loanApplicationId == Guid.Empty)
            {
                return Failure("Application ID cannot be empty");
            }

            var checklist = await GetChecklistAsync(
                loanApplicationId,
                request.DocumentTypeCode);

            if (checklist == null)
            {
                return Failure(
                    "Invalid Application ID or Document Type Code");
            }

            var verificationError = ValidateChecklistStatus(checklist);

            if (verificationError != null)
            {
                return Failure(verificationError);
            }

            var fileValidationError = ValidateFile(
                request.File,
                checklist);

            if (fileValidationError != null)
            {
                return Failure(fileValidationError);
            }

            await RemoveExistingDocumentIfNecessary(checklist);

            var uploadResult = await _uploadService.UploadDocumentAsync(
                request.File,
                loanApplicationId,
                checklist.DocumentTypeCode);

            if (!uploadResult.IsSuccess)
            {
                return Failure(uploadResult.ErrorMessage!);
            }

            var loanDocument = CreateLoanDocument(
                request,
                checklist,
                uploadResult.StoragePath!,
                loanApplicationId);

            try
            {
                await PersistLoanDocumentAsync(
                    loanDocument,
                    checklist);

                return MapResponse(loanDocument);
            }
            catch
            {
                await _uploadService.DeleteDocumentAsync(
                    uploadResult.StoragePath!);

                return Failure(
                    "Error saving file details to database");
            }
        }

        private async Task<ApplicationDocumentChecklist?> GetChecklistAsync(Guid loanApplicationId, string documentTypeCode)
        {
            return await _checklistRepo.Query()
                .Include(x => x.LoanApplication)
                .ThenInclude(x => x.Applicant)
                .FirstOrDefaultAsync(x =>
                    x.LoanApplicationId == loanApplicationId &&
                    x.DocumentTypeCode == documentTypeCode);
        }
        private static string? ValidateChecklistStatus(ApplicationDocumentChecklist checklist)
        {
            if (checklist.Status == ChecklistItemStatus.Verified)
            {
                return "Cannot re-upload verified document";
            }

            return null;
        }

        private static string? ValidateFile(IFormFile file, ApplicationDocumentChecklist checklist)
        {
            var allowedExtensions = checklist.AllowedFileTypes
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => "." + x.Trim().ToLower())
                .ToList();

            var uploadedExtension =
                Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(uploadedExtension))
            {
                return $"Invalid file extension. Only " +
                       $"{checklist.AllowedFileTypes} " +
                       $"are allowed for " +
                       $"{checklist.DocumentTypeCode}";
            }

            long maxFileSizeBytes =
                (long)checklist.MaxFileSizeMb * 1024 * 1024;

            if (file.Length > maxFileSizeBytes)
            {
                return $"File size exceeds limit. " +
                       $"'{checklist.DocumentTypeName}' " +
                       $"cannot exceed " +
                       $"{checklist.MaxFileSizeMb}MB.";
            }

            return null;
        }

        private static LoanDocumentUploadResponse Failure(string message)
        {
            return BaseResponse
                .Failure<LoanDocumentUploadResponse>(
                    message: message);
        }

        private static LoanDocumentUploadResponse MapResponse(LoanDocument loanDocument)
        {
            return new LoanDocumentUploadResponse
            {
                DocumentId = loanDocument.Id,

                DocumentTypeCode = loanDocument.DocumentTypeCode,
                DocumentTypeName = loanDocument.DocumentTypeName,

                FileName = loanDocument.FileName,

                Status = loanDocument.Status.ToString(),

                UploadedAt = loanDocument.UploadedAt,

                IsSuccess = true,
                Message = "Document uploaded successfully!"
            };
        }

        private async Task PersistLoanDocumentAsync(LoanDocument loanDocument, ApplicationDocumentChecklist checklist)
        {
            await _loanDocumentRepository.AddAsync(
                loanDocument);

            checklist.Status = ChecklistItemStatus.Uploaded;
            checklist.LoanDocumentId = loanDocument.Id;
            checklist.UpdatedAt = DateTime.UtcNow;

            await _loanDocumentRepository.SaveChangesAsync();
        }

        private static LoanDocument CreateLoanDocument(LoanDocumentUploadRequest request, ApplicationDocumentChecklist checklist, string storagePath, Guid loanApplicationId)
        {
            return new LoanDocument
            {
                LoanApplicationId = loanApplicationId,
                ApplicantId = checklist.LoanApplication.ApplicantId,

                DocumentTypeCode = checklist.DocumentTypeCode,
                DocumentTypeName = checklist.DocumentTypeName,

                FileName = request.FileName,
                StoragePath = storagePath,

                FileSize = request.File.Length,
                ContentType = request.File.ContentType,

                Status = DocumentStatus.Pending,

                UploadedAt = DateTime.UtcNow,

                LoanApplication = checklist.LoanApplication,
                Applicant = checklist.LoanApplication.Applicant
            };
        }

        private async Task RemoveExistingDocumentIfNecessary(ApplicationDocumentChecklist checklist)
        {
            if (checklist.Status != ChecklistItemStatus.Uploaded &&
                checklist.Status != ChecklistItemStatus.Rejected)
            {
                return;
            }

            var existingDocument =
                await _loanDocumentRepository.Query()
                    .FirstOrDefaultAsync(x =>
                        x.Id == checklist.LoanDocumentId);

            if (existingDocument == null)
            {
                return;
            }

            await _uploadService.DeleteDocumentAsync(
                existingDocument.StoragePath);

            existingDocument.IsDeleted = true;

            _loanDocumentRepository.Update(existingDocument);

            await _loanDocumentRepository.SaveChangesAsync();
        }
    }
}
