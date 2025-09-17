using OnePortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Abstractions
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken?> FindActiveByHashAsync(string tokenHash, CancellationToken ct);
        Task UpdateAsync(PasswordResetToken token, CancellationToken ct);
    }
}
