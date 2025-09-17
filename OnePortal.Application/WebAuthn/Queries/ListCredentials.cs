using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.WebAuthn.Dtos;

namespace OnePortal.Application.WebAuthn.Queries;

public record ListCredentialsQuery() : IRequest<List<WebAuthnCredentialDto>>;

public class ListCredentialsHandler : IRequestHandler<ListCredentialsQuery, List<WebAuthnCredentialDto>>
{
    private readonly IWebAuthnCredentialRepository _creds;
    private readonly ICurrentUser _current;

    public ListCredentialsHandler(IWebAuthnCredentialRepository creds, ICurrentUser current)
    {
        _creds = creds;
        _current = current;
    }

    public async Task<List<WebAuthnCredentialDto>> Handle(ListCredentialsQuery r, CancellationToken ct)
    {
        var userId = _current.UserId ?? throw new UnauthorizedAccessException();

        var creds = await _creds.GetActiveByUserAsync(userId, ct);

        return creds.Select(c => new WebAuthnCredentialDto(
            c.CredentialId,
            c.Aaguid.ToString(),
            c.CreatedUtc,
            c.SignCount,
            (c.TransportsCsv ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        )).ToList();
    }
}
