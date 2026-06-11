using LoanRequestApplication.Interfaces.Services;
using LoanRequestInfrastructure.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanRequestAPI.Controllers.Auth
{
    /// <summary>
    /// Handles user identity verification, secure authentication, session management, and credential recovery for LoanQuest.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register a new user account.
        /// </summary>
        /// <remarks>
        /// Creates a new account in the system and automatically sends a verification code (OTP) to the user's email so they can activate it.
        /// </remarks>
        /// <param name="request">The user's sign-up information.</param>
        /// <response code="200">Account created successfully and verification email sent.</response>
        /// <response code="400">Registration failed. The email might already be taken, or the password is too weak.</response>
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            var response = await _authService.RegisterUserAsync(request);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }
        /// <summary>
        /// Authenticate user credentials and issue active secure access tokens.
        /// </summary>
        /// <remarks>
        /// On a valid match, returns an ephemeral JWT Access Token (15-minute validity) passed via JSON body payloads, 
        /// along with a highly protected, long-lived Refresh Token intended for secure client-side preservation.
        /// </remarks>
        /// <param name="request">Username/Email credentials alongside standard password inputs.</param>
        /// <response code="200">Authentication successful; access token and refresh structures returned.</response>
        /// <response code="400">Invalid authentication credentials supplied, or account currently locked out.</response>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var response = await _authService.LoginUserAsync(request);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Refresh an expired login session using a refresh token.
        /// </summary>
        /// <remarks>
        /// Takes the user's current refresh token and swaps it for a brand new access token and refresh token. This keeps the user logged in without forcing them to type their password again. If an old token is reused, the system will block it for security.
        /// </remarks>
        /// <param name="oldrefreshToken">The current refresh token string.</param>
        /// <param name="userId">The ID of the user requesting the new token.</param>
        /// <response code="200">Token swapped successfully. Returns a new set of tokens.</response>
        /// <response code="400">Token exchange denied. The token might be expired, invalid, or already used.</response>
        [HttpPost("refresh")]
        public async Task<ActionResult<RotateTokensResponseDto>> RotateTokens([FromBody] string oldrefreshToken, string userId)
        {
            var response = await _authService.RotateKeysAsync(oldrefreshToken, userId);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Log the user out and end their session.
        /// </summary>
        /// <remarks>
        /// Deletes the current active session in the database and blacklists the tokens so they can never be used again.
        /// </remarks>
        /// <response code="200">Logged out successfully. Tokens are now invalid.</response>
        /// <response code="401">Unauthorized. You must pass a valid JWT token to log out.</response>
        [Authorize]
        [HttpPost("logout")]
        public async Task<LogoutResponseDto> Logout()
        {
            return await _authService.LogoutUserAsync();
        }

        /// <summary>
        /// Request a password reset link.
        /// </summary>
        /// <remarks>
        /// Checks if the email exists. If it does, it sends the new tokens in the response. Temporarily because Email channels have not been setup
        /// </remarks>
        /// <param name="request">The email address of the account needing a password reset.</param>
        /// <response code="200">Request processed. If the email exists, a reset link was sent.</response>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var response = await _authService.SendUserPasswordResetLink(request.Email);
            return Ok(response);
        }

        /// <summary>
        /// Complete the password reset using the token from the email.
        /// </summary>
        /// <remarks>
        /// Takes the security token sent to the user's email along with their new password. If the token is valid and hasn't expired, it updates the account with the new password.
        /// </remarks>
        /// <param name="request">The secure token from the email and the new password choice.</param>
        /// <response code="200">Password updated successfully. The user can now log in with their new password.</response>
        /// <response code="400">Failed to update password. The token might be expired or invalid.</response>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var response = await _authService.ResetUserPassword(request);
            return Ok(response);
        }


    }
}
