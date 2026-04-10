using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestInfrastructure.Services.Auth;
using LoanRequestDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanRequestInfrastructure.Persistence.Repositories
{
    public class RefreshTokensRepository : IRefreshTokensRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokensRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task CreateRefreshTokenAsync(CreateRefreshTokensDto newTokens)
        {
            await _context.RefreshTokens.AddAsync(new RefreshToken
            {
                UserId = newTokens.UserId,
                DeviceInfo = newTokens.DeviceInfo,
                Token = newTokens.Token,
                ExpiresAt = newTokens.ExpiresAt
            });

            await _context.SaveChangesAsync();
        }

        public async Task<string> GetRefreshTokenAsync(string userId)
        {
            var now = DateTime.UtcNow;

            var currentActiveToken = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId
                          && rt.RevokedAt == null
                          && rt.ExpiresAt > now) 
                .OrderByDescending(rt => rt.CreatedAt) 
                .FirstOrDefaultAsync();

            return currentActiveToken?.Token ?? string.Empty;
        }

        public async Task<bool> IsRefreshTokenValidAsync(string token, string UserId)
        {
            var now = DateTime.UtcNow;
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => (rt.Token == token) && rt.UserId == UserId);
            return refreshToken != null 
                   && refreshToken.RevokedAt == null 
                   && refreshToken.ExpiresAt > now;
        }

        public async Task<RefreshToken?> GetAndUpdateTokenAsync(string token, string userId)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.UserId == userId);

            if (storedToken == null) return null;

            if (storedToken.RevokedAt == null && storedToken.ExpiresAt > DateTime.UtcNow)
            {
                storedToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return storedToken;
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken != null)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeAllTokensForUserAsync(string userId)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
