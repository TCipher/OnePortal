using System.Security.Cryptography;
using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;


namespace OnePortal.Application.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest<bool>;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
}

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailSender _email;
    private readonly IConfigurationService _config;
    private readonly IUserRepository _users;

    public ForgotPasswordHandler(IUnitOfWork uow, IEmailSender email, IConfigurationService config, IUserRepository users)
    {
        _uow = uow; _email = email; _config = config; _users = users;
    }

    public async Task<bool> Handle(ForgotPasswordCommand r, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(r.Email, ct);
        if (user is null) return true; // don't reveal

        // generate token
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(bytes);
        var tokenHash = Hash(token);
        var expires = DateTime.UtcNow.AddMinutes(15);

        await _uow.Repository<PasswordResetToken>().AddAsync(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresUtc = expires
        }, ct);
        await _uow.SaveChangesAsync(ct);

        var baseUrl = _config.GetResetPasswordBaseUrl();
        var link = $"{baseUrl}?token={Uri.EscapeDataString(token)}";
        await _email.SendAsync(user.EmailAddress, "Reset your OnePortal password",
            $"<p>Click to reset your password: <a href=\"{link}\">Reset Password</a><br/>This link expires in 15 minutes.</p>", ct);

        return true;
    }

    private static string Hash(string token)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token)));
    }
}
