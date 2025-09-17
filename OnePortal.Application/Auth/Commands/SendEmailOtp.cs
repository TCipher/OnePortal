using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;


namespace OnePortal.Application.Auth.Commands;

public record SendEmailOtpCommand(string Email, string? ChallengeId) : IRequest<(bool Ok, int ExpiresIn)>;

public class SendEmailOtpValidator : AbstractValidator<SendEmailOtpCommand>
{
    public SendEmailOtpValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class SendEmailOtpHandler : IRequestHandler<SendEmailOtpCommand, (bool Ok, int ExpiresIn)>
{
    private readonly IOtpService _otp;
    public SendEmailOtpHandler(IOtpService otp) => _otp = otp;

    public Task<(bool Ok, int ExpiresIn)> Handle(SendEmailOtpCommand r, CancellationToken ct)
        => _otp.SendEmailOtpAsync(r.Email, r.ChallengeId, ct);
}
