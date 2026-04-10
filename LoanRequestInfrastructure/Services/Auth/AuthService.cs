using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using LoanRequestApplication.DTOs;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestApplication.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace LoanRequestInfrastructure.Services.Auth
{
    public class AuthService :  BaseService, IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;

        public AuthService(UserManager<IdentityUser> userManager, ITokenService tokenService, IRefreshTokensRepository refreshTokensRepository, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) 
        {
        
            _userManager = userManager;
            _tokenService = tokenService;
            _refreshTokensRepository = refreshTokensRepository;
        }

        public async Task<RegisterResponseDto> RegisterUserAsync(RegisterRequestDto request)
        {
            var user = new IdentityUser
            {
                UserName = request.Username,
                Email = request.Email
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                return new RegisterResponseDto { IsSuccess = true, Message = "User registered successfully." };
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new RegisterResponseDto { IsSuccess = false, Message = $"Registration failed: {errors}" };
            }
        }

        public async Task<LoginResponseDto> LoginUserAsync(LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, request.Password))
            {
                var tokens = await _tokenService.CreateToken(user);
                return new LoginResponseDto { IsSuccess = true, Message = "User Login Successfully", Token = tokens };
            }
            return new LoginResponseDto { IsSuccess = false, Message = "Invalid username or password." };
        }

        public async Task<RotateTokensResponseDto> RotateKeysAsync(string oldRefreshToken, string UserId)
        {
            var storedToken = await _refreshTokensRepository.GetAndUpdateTokenAsync(oldRefreshToken, UserId);
            if (storedToken == null)
                return new RotateTokensResponseDto { IsSuccess = false, Message = "Token not found" };

            if (storedToken.RevokedAt != null && storedToken.ExpiresAt > DateTime.UtcNow && storedToken.RevokedAt < DateTime.UtcNow.AddSeconds(-1))
            {
                // If it was ALREADY revoked before this call, someone is trying to reuse it!
                return new RotateTokensResponseDto { IsSuccess = false, Message = "Security Alert: Token reuse detected" };
            }

            if (storedToken.ExpiresAt < DateTime.UtcNow)
                return new RotateTokensResponseDto { IsSuccess = false, Message = "Token expired" };

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
                return new RotateTokensResponseDto { IsSuccess = false, Message = "User not found" };

            var newTokens = await _tokenService.CreateToken(user);
            return new RotateTokensResponseDto { IsSuccess = true, Message = "Tokens rotated", Token = newTokens };
        }

        public async Task<LogoutResponseDto> LogoutUserAsync()
        {
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                return new LogoutResponseDto { IsSuccess = false, Message = "User not identified." };
            }

            await _refreshTokensRepository.RevokeAllTokensForUserAsync(CurrentUserId);

            return new LogoutResponseDto { IsSuccess = true, Message = "Logged out." };
        }

        public async Task<object> SendUserPasswordResetLink(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return new { Message = "If an account exists, a reset link has been sent." };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    
           // var resetLink = $"https://custom-url-app.com/reset-password?token={Uri.EscapeDataString(token)}&email={user.Email}";
           //  await _emailService.SendEmailAsync(user.Email, "Reset Your Password", $"Click here: {resetLink}");

            return new { Message = "If an account exists, a reset link has been sent.", Token = token };
        }

        public async Task<ResetPasswordResponse> ResetUserPassword(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return new ResetPasswordResponse { IsSuccess = false, Message = "Email not found." };
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded) { return new ResetPasswordResponse { IsSuccess = false, Message = "Password reset failed" }; }
            else { return new ResetPasswordResponse { IsSuccess = true, Message = "Password has been reset successfully." }; }
        }
    }
}
