using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LoanRequestApplication.DTOs;
using LoanRequestDomain.Entities;
using LoanRequestDomain.Enums;

namespace LoanRequestApplication.Profiles
{
    public class DocumentMappingProfile : Profile
    {
        public DocumentMappingProfile()
        {
            CreateMap<DocumentRequirement, ApplicationDocumentChecklist>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.LoanApplicationId, opt => opt.Ignore())

            // Set default initial states
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ChecklistItemStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<ApplicationDocumentChecklist, DocumentChecklistItemDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.UploadedDocumentId, opt => opt.MapFrom(src => src.LoanDocumentId));
        }
    }
}
