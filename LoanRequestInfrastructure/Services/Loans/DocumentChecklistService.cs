using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestApplication.Interfaces.Services;
using LoanRequestDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanRequestInfrastructure.Services.Loans
{
    public class DocumentChecklistService : IDocumentChecklistService
    {
        private readonly IGenericRepository<ApplicationDocumentChecklist> _checklistRepo;
        private readonly IDocumentRequirementRepository _documentRequirementsRepository;
        private readonly IMapper _mapper;

        public DocumentChecklistService(IGenericRepository<ApplicationDocumentChecklist> checklistRepo, IDocumentRequirementRepository documentRequirementsRepository, IMapper mapper)
        {
            _checklistRepo = checklistRepo;
            _documentRequirementsRepository = documentRequirementsRepository;
            _mapper = mapper;
        }

        public async Task<bool> CreateChecklistForApplication(Guid applicationId, Guid productId)
        {
            // 1. Get the requirements for the chosen product
            var documentRequirements = await _documentRequirementsRepository.GetDocumentRequirementsByLoanProductIdAsync(productId);

            // 2. Map the template to the actual checklist items
            var checklistItems = _mapper.Map<List<ApplicationDocumentChecklist>>(documentRequirements);

            // 3. Assign the specific Application ID to all items
            foreach (var item in checklistItems)
            {
                item.LoanApplicationId = applicationId;
            }

            // 4. Save to DB
            await _checklistRepo.AddRangeAsync(checklistItems);
            bool saved = await _checklistRepo.SaveChangesAsync();
            return saved;
        }

        public async Task<bool> CheckApplicantDocumentSnapshot(Guid applicationId)
        {
            bool result = await _checklistRepo.Query().AnyAsync(x => x.LoanApplicationId == applicationId);
            return result;
        }

        public async Task<IEnumerable<ApplicationDocumentChecklist>?> GetChecklistForApplication(Guid applicationId)
        {
            var result = await _checklistRepo.Query()
                .Where(ac => ac.LoanApplicationId == applicationId)
                .ToListAsync();

            return result;
        }
    }
    }
