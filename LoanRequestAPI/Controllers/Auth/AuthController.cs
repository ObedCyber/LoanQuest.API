using LoanRequestApplication.Interfaces.Services;
using LoanRequestInfrastructure.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LoanRequestAPI.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            var response = await _authService.RegisterUserAsync(request);
            if(!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var response = await _authService.LoginUserAsync(request);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<RotateTokensResponseDto>> RotateTokens([FromBody] string oldrefreshToken, string userId) 
        { 
            var response = await _authService.RotateKeysAsync(oldrefreshToken, userId);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<LogoutResponseDto> Logout()
        {
            return await _authService.LogoutUserAsync();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var response = await _authService.SendUserPasswordResetLink(request.Email);
            return Ok(response);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var response = await _authService.ResetUserPassword(request);
            return Ok(response);
        }


    }
}
