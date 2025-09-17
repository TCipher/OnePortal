using OnePortal.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace OnePortal.Infrastructure.Security
{
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<UserDetails> _hasher = new();
        public string Hash(string password) => _hasher.HashPassword(default!, password);
        public bool Verify(string? hash, string password)
            => hash is not null && _hasher.VerifyHashedPassword(default!, hash, password) is not PasswordVerificationResult.Failed;
    }
}
