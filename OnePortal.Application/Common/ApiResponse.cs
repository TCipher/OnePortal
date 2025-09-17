using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Common
{
    public record ApiResponse<T>(bool ok, T? data, ApiError? error = null)
    {
        public static ApiResponse<T> Success(T data) => new(true, data, null);
        public static ApiResponse<T> Fail(string code, string message, object? details = null)
            => new(false, default, new ApiError(code, message, details));
    }

    public record ApiError(string code, string message, object? details = null);
}
