using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestApplication.DTOs;
using LoanRequestInfrastructure.Services.Auth;

namespace LoanRequestApplication.Interfaces.Services
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterUserAsync(RegisterRequestDto request);
        Task<LoginResponseDto> LoginUserAsync(LoginRequestDto request);
        Task<RotateTokensResponseDto> RotateKeysAsync(string oldRefreshToken, string UserId);
        Task<LogoutResponseDto> LogoutUserAsync();
        Task<object> SendUserPasswordResetLink(string email);
        Task<ResetPasswordResponse> ResetUserPassword(ResetPasswordRequest request);



    }
}
