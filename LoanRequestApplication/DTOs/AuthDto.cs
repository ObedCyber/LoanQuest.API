using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.DTOs;

namespace LoanRequestInfrastructure.Services.Auth
{
    public class RegisterRequestDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class RegisterResponseDto : BaseResponse { }

    public class LogoutResponseDto :BaseResponse { }

    public class LoginRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class LoginResponseDto : BaseResponse
    {
        public TokenResponseDto? Token { get; set; }
    }

    public class CreateRefreshTokensDto
    {
        public required string UserId { get; set; }
        public string? DeviceInfo { get; set; }
        public required string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class RotateTokensResponseDto : BaseResponse
    {
        public TokenResponseDto? Token { get; set; }
    }

    public class TokenResponseDto
    {
        public string JwtToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        [EmailAddress]
        public required string Email { get; set; }
    }
    
    public class ResetPasswordRequest
    {
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string NewPassword { get; set; }

        [Required]
        public required string Token { get; set; }
    }

    public class ResetPasswordResponse : BaseResponse
    {

    }
}
