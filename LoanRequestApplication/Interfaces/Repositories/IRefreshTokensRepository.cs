using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestDomain.Entities;
using LoanRequestInfrastructure.Services.Auth;

namespace LoanRequestApplication.Interfaces.Repositories
{
    public interface IRefreshTokensRepository
    {
        Task<string> GetRefreshTokenAsync(string userId);
        Task CreateRefreshTokenAsync(CreateRefreshTokensDto newTokens);
        Task RevokeRefreshTokenAsync(string Token);
        Task RevokeAllTokensForUserAsync(string userId);
        Task<bool> IsRefreshTokenValidAsync(string token, string UserId);
        Task<RefreshToken?> GetAndUpdateTokenAsync(string token, string userId);

    }
}
