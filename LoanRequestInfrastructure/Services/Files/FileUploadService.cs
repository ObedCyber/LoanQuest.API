using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using LoanRequestApplication.DTOs;
using LoanRequestApplication.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace LoanRequestInfrastructure.Services.Files
{
    public class FileUploadService : IFileUploadService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly BlobServiceClient _blobServiceClient;

        public FileUploadService(IConfiguration configuration, BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;

            var connectionString = configuration["AzureBlobStorage:ConnectionString"];

            var containerName = configuration["AzureBlobStorage:ContainerName"];

            _blobServiceClient = new BlobServiceClient(connectionString);

            _containerClient =  blobServiceClient.GetBlobContainerClient(containerName);

            _containerClient.CreateIfNotExists(
                PublicAccessType.None
            );
        }
        public async Task<BlobUploadResult> UploadDocumentAsync(IFormFile file, Guid loanApplicationId, string docTypeCode)
        {
            {
                // Failsafe 1: Defensive parameter checking before touching network resources
                if (file == null || file.Length == 0)
                {
                    return new BlobUploadResult { IsSuccess = false, ErrorMessage = "File stream is empty or null." };
                }

                try
                {
                    string fileExtension = Path.GetExtension(file.FileName);
                    string uniqueBlobName = $"loans/{loanApplicationId}/{docTypeCode}_{Guid.NewGuid()}{fileExtension}";

                    var blobClient = _containerClient.GetBlobClient(uniqueBlobName);

                    var blobHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    };

                    // Ensure container exists lazily
                    await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

                    using var stream = file.OpenReadStream();

                    // Capture the Azure Response object explicitly
                    Response<BlobContentInfo> response = await blobClient.UploadAsync(
                        stream,
                        new BlobUploadOptions { HttpHeaders = blobHeaders }
                    );

                    // Verify the HTTP response status code from Azure endpoints (201 Created)
                    if (response.GetRawResponse().Status == 201 || response.GetRawResponse().Status == 200)
                    {
                        return new BlobUploadResult
                        {
                            IsSuccess = true,
                            StoragePath = uniqueBlobName,
                        };
                    }

                    // Handle edge cases where Azure replies but HTTP status indicates failure
                    return new BlobUploadResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Azure responded with abnormal status code: {response.GetRawResponse().Status}"
                    };
                }
                catch (RequestFailedException ex)
                {
                    // Catch specific Azure Storage SDK connection/permission exceptions
                    return new BlobUploadResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Azure Storage error: {ex.ErrorCode} - {ex.Message}"
                    };
                }
                catch (Exception ex)
                {
                    //  Catch general infrastructure issues (e.g., local IFormFile stream failure)
                    return new BlobUploadResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Internal upload system exception: {ex.Message}"
                    };
                }
            }
        }

        public async Task DeleteDocumentAsync(string blobPath)
        { 
            var blobClient = _containerClient.GetBlobClient(blobPath);

            await blobClient.DeleteIfExistsAsync();
        }

    }

    
}
