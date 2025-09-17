using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Abstractions
{
    public interface IOtpService
    {
        Task<(bool Ok, int ExpiresIn)> SendEmailOtpAsync(string email, string? challengeId, CancellationToken ct);
        Task<bool> VerifyEmailOtpAsync(string email, string otp, string? challengeId, CancellationToken ct);
    }
}
