using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LoanRequestApplication.DTOs;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestDomain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LoanRequestInfrastructure.Services.Applicants
{
    public class EmploymentService : BaseService, IEmploymentService
    {
        private readonly IMapper _mapper;
        private readonly IGenericRepository<ApplicantEmployment> _repository;
        private readonly IApplicantRepository _ApplicantRepository;

        public EmploymentService(IMapper mapper, IApplicantRepository Applicantrepository, IGenericRepository<ApplicantEmployment> repository, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _mapper = mapper;
            _repository = repository;
            _ApplicantRepository = Applicantrepository;
        }

        public async Task<EmploymentResponse> AddApplicantEmploymentDetails(EmploymentRequest request)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                return BaseResponse.Failure<EmploymentResponse>("User session is invalid.");
            }

           if (CurrentApplicantId == Guid.Empty) return BaseResponse.Failure<EmploymentResponse>("Applicant profile not found.");
           var bvn = await _ApplicantRepository.Query().Where(x => x.Id == CurrentApplicantId).Select(x => x.BVN).FirstOrDefaultAsync();
           if (bvn == "") return BaseResponse.Failure<EmploymentResponse>("BVN has not been Verified");
           bool EmploymentDetailsExist = await _repository.Query().AnyAsync(x => x.ApplicantId == CurrentApplicantId);

           if (EmploymentDetailsExist) return BaseResponse.Failure<EmploymentResponse>("Applicant Employment Details already Exists"); ;

            var employmentEntity = _mapper.Map<ApplicantEmployment>(request);

            employmentEntity.CreatedAt = DateTime.UtcNow;

            employmentEntity.ApplicantId = CurrentApplicantId;

            try
            {
                await _repository.AddAsync(employmentEntity);
                bool saved = await _repository.SaveChangesAsync();

                if (!saved)
                {
                    return BaseResponse.Failure<EmploymentResponse>("Failed to save employment details.");
                }

                var responseData = _mapper.Map<EmploymentDetailsDto>(employmentEntity);

                return new EmploymentResponse
                {
                    IsSuccess = true,
                    Message = "Employment details added successfully.",
                    Data = responseData
                };
            }
            catch (Exception ex)
            {
                return BaseResponse.Failure<EmploymentResponse>($"An error occurred: {ex.Message}");
            }
        }
    }
}
