using FluentValidation;
using MediatR;
using OnePortal.Application.Auth.Dtos;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResultDto>;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, LoginResultDto>
{
    private readonly IRefreshTokenService _service;
    private readonly ITokenService _tokens;
    private readonly IUserRepository _user;

    public RefreshTokenHandler(IRefreshTokenService service, ITokenService tokens, IUserRepository user)
    { _service = service; _tokens = tokens; _user = user; }

    public async Task<LoginResultDto> Handle(RefreshTokenCommand r, CancellationToken ct)
    {
        var rotated = await _service.RotateAsync(r.RefreshToken, ct);
        if (!rotated.ok) throw new UnauthorizedAccessException("Invalid refresh token");

        var user = await _user.GetByIdAsync(rotated.userId,ct);
        if (user == null) throw new UnauthorizedAccessException("User not found");
        var roleCode = user.Role?.Code ?? GlobalRoleCodes.User;
        var fullName = string.Join(" ", new[] { user.FirstName, user.MiddleName, user.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));

        var access = _tokens.CreateToken(user.Id, user.EmailAddress, fullName, roleCode);
        return LoginResultDto.Issued(access, 1200, rotated.newToken, 1200);
    }
}
