using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRequestApplication.DTOs
{
    public abstract class BaseResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;

        // Helper methods for cleaner service code
        public static T Success<T>(string message = "Success") where T : BaseResponse, new()
            => new T { IsSuccess = true, Message = message };

        public static T Failure<T>(string message) where T : BaseResponse, new()
            => new T { IsSuccess = false, Message = message };
    }
}
