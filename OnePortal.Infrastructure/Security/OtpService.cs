using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using OnePortal.Application.Abstractions;
using OnePortal.Infrastructure.Data;
using OnePortal.Infrastructure.Persistence;



public class OtpService : IOtpService
{
    private const int TtlSeconds = 600; // 10 min
    private const int CooldownSeconds = 30;
    private const int MaxAttempts = 5;
    private readonly IEmailSender _email;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    public OtpService(IUserRepository users, IEmailSender email, IUnitOfWork uow) {_email = email;_users = users;_uow = uow;}

    public async Task<(bool Ok, int ExpiresIn)> SendEmailOtpAsync(string email, string? challengeId, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(email, ct)
                   ?? throw new UnauthorizedAccessException("Unknown email");

        if (user.LockoutEnabled && user.LockoutEndUtc is not null && user.LockoutEndUtc > DateTime.UtcNow)
            throw new InvalidOperationException("Locked");

        if (user.EmailOtpLastSentUtc is not null && (DateTime.UtcNow - user.EmailOtpLastSentUtc.Value).TotalSeconds < CooldownSeconds)
            throw new InvalidOperationException("Too many requests");

        // generate numeric code
        var code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
        // hash with SHA256(email + code + per-user random salt)
        var salt = Guid.NewGuid().ToString("N");
        var toHash = $"{email}:{code}:{salt}";
        using var sha = SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(toHash)));

        user.EmailOtpHash = $"{salt}:{hash}";
        user.EmailOtpExpiresUtc = DateTime.UtcNow.AddSeconds(TtlSeconds);
        user.EmailOtpLastSentUtc = DateTime.UtcNow;
        user.EmailOtpFailedCount = 0;

        await _uow.SaveChangesAsync(ct);

        await _email.SendAsync(email, "Your OnePortal verification code",
            $"<p>Your verification code is <b>{code}</b>. It expires in 10 minutes.</p>", ct);

        return (true, TtlSeconds);
    }

    public async Task<bool> VerifyEmailOtpAsync(string email, string otp, string? challengeId, CancellationToken ct)
    {
        var user = await  _users.GetByEmailAsync(email, ct)
                   ?? throw new UnauthorizedAccessException();

        if (user.EmailOtpExpiresUtc is null || user.EmailOtpExpiresUtc < DateTime.UtcNow)
            return false;

        if (user.EmailOtpHash is null) return false;

        var parts = user.EmailOtpHash.Split(':');
        var salt = parts[0];
        using var sha = SHA256.Create();
        var check = Convert.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"{email}:{otp}:{salt}")));
        var ok = string.Equals($"{salt}:{check}", user.EmailOtpHash, StringComparison.Ordinal);

        if (!ok)
        {
            user.EmailOtpFailedCount++;
            if (user.EmailOtpFailedCount >= MaxAttempts)
                user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(15);
            await _uow.SaveChangesAsync(ct);
            return false;
        }

        // success → clear OTP
        user.EmailOtpHash = null;
        user.EmailOtpExpiresUtc = null;
        user.EmailOtpFailedCount = 0;
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
