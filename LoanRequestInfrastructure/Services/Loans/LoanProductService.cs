using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LoanRequestApplication.DTOs;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestApplication.Interfaces.Services;
using LoanRequestDomain.Entities;

namespace LoanRequestInfrastructure.Services.Loans
{
    public class LoanProductService : ILoanProductService
    {
        private readonly IGenericRepository<LoanProducts> _loanProductRepository;
        private readonly IMapper _mapper;

        public LoanProductService(IGenericRepository<LoanProducts> loanProductRepository, IMapper mapper)
        {
            _loanProductRepository = loanProductRepository;
            _mapper = mapper;
        }
        public async Task<LoanProductFetchResponse> GetLoanProductsAsync() 
        {
            var loanProducts = await _loanProductRepository.GetAllAsync();            
            var responseData = _mapper.Map<IEnumerable<LoanProductResponseDto>>(loanProducts.Where(lp => lp.IsActive));
            return new LoanProductFetchResponse { IsSuccess = true, Message = "Active loan products retrieved successfully", Data = responseData };
        }

        public async Task<LoanProducts?> GetLoanProductByIdAsync(Guid id)
        {
            var loanProduct = await _loanProductRepository.GetByIdAsync(id);
            return loanProduct;
        }

        public async Task<LoanProducts?> GetLoanProductByProductCode(string Code)
        {
            var loanProducts = await _loanProductRepository.GetAllAsync();
            return loanProducts.FirstOrDefault(pc => pc.ProductCode == Code);
        }
    }
}
