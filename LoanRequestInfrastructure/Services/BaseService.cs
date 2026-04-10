using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace LoanRequestInfrastructure.Services
{
    public abstract class BaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Centrally managed property to get the Current User ID
        protected string? CurrentUserId =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.NameId)
            ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        // Helper to check if the user is authenticated
        protected bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        // Property to get the Client's IP Address
        protected string? CurrentIpAddress =>
            _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        // Property to get the Applicant's ID
        protected Guid CurrentApplicantId
        {
            get
            {
                var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirstValue("ApplicantId");

                if (Guid.TryParse(claimValue, out var applicantId))
                {
                    return applicantId;
                }
                return Guid.Empty;
            }
        }

    }
}
