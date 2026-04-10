using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestApplication.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LoanRequestInfrastructure.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IApplicantRepository _applicantRepository;

        public TokenService(IConfiguration config, IRefreshTokensRepository refreshTokensRepository, IApplicantRepository applicantRepository)
        {
            _config = config;
            _refreshTokensRepository = refreshTokensRepository;
            _applicantRepository = applicantRepository;
        }

        public async Task<TokenResponseDto> CreateToken(IdentityUser user)
        {
            var AccessToken = await GenerateJwtToken(user); // existing JWT logic
            var RefreshToken = await  CreateNewRefreshToken(user.Id); // A long random string
            return new TokenResponseDto
            {
                JwtToken = AccessToken,
                RefreshToken = RefreshToken
            };
        }

        private async Task<string> CreateNewRefreshToken(string userId)
        {
            var existingToken = await _refreshTokensRepository.GetRefreshTokenAsync(userId);
            if (!string.IsNullOrEmpty(existingToken))
            {
                await _refreshTokensRepository.RevokeRefreshTokenAsync(existingToken);
            }
            var refreshToken = GenerateSecureRandomToken();
            var newRefreshToken = new CreateRefreshTokensDto
            {
                UserId = userId,
                DeviceInfo = "Unknown Device",
                Token = refreshToken, 
                ExpiresAt = DateTime.UtcNow.AddDays(7) 
            };
            await _refreshTokensRepository.CreateRefreshTokenAsync(newRefreshToken);
            return newRefreshToken.Token;
        }

        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var applicant = await _applicantRepository.GetApplicantByUserIdAsync(user.Id);
            string applicantId = string.Empty; 
            if(applicant != null ) applicantId = applicant.Id.ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("ApplicantId", applicantId), 
                new Claim("ProjectName", "LoanRequestSystem") 
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing from appsettings.json")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateSecureRandomToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

    }
}
