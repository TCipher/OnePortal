using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.AuditLogs.Dtos;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.AuditLogs.Queries;

public record GetAuditLogsForPortalQuery(int PortalId, int Take = 200) : IRequest<List<AuditLogDto>>;

public class GetAuditLogsForPortalValidator : AbstractValidator<GetAuditLogsForPortalQuery>
{
    public GetAuditLogsForPortalValidator()
    {
        RuleFor(x => x.PortalId).GreaterThan(0);
        RuleFor(x => x.Take).InclusiveBetween(1, 1000);
    }
}

public class GetAuditLogsForPortalHandler : IRequestHandler<GetAuditLogsForPortalQuery, List<AuditLogDto>>
{
    private readonly IAuditLogRepository _repo;
    public GetAuditLogsForPortalHandler(IAuditLogRepository repo) => _repo = repo;

    public async Task<List<AuditLogDto>> Handle(GetAuditLogsForPortalQuery req, CancellationToken ct)
    {
        return await _repo.GetForPortalAsync(req.PortalId, req.Take, ct);
    }
}