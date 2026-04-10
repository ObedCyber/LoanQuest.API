using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanRequestInfrastructure.Services.Auth;
using Microsoft.AspNetCore.Identity;

namespace LoanRequestApplication.Interfaces.Services
{
    public interface ITokenService
    {
        Task<TokenResponseDto> CreateToken(IdentityUser user);
    }
}
