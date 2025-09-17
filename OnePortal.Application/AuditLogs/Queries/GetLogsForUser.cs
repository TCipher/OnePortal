using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.AuditLogs.Dtos;
using OnePortal.Domain.Entities;


namespace OnePortal.Application.AuditLogs.Queries;

public record GetAuditLogsForUserQuery(int UserId, int Take = 200) : IRequest<List<AuditLogDto>>;

public class GetAuditLogsForUserValidator : AbstractValidator<GetAuditLogsForUserQuery>
{
   
        public GetAuditLogsForUserValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0);
            RuleFor(x => x.Take).InclusiveBetween(1, 1000);
        }
    
}

public class GetAuditLogsForUserHandler : IRequestHandler<GetAuditLogsForUserQuery, List<AuditLogDto>>
{
    private readonly IAuditLogRepository _repo;
    public GetAuditLogsForUserHandler(IAuditLogRepository repo) => _repo = repo;

    public async Task<List<AuditLogDto>> Handle(GetAuditLogsForUserQuery req, CancellationToken ct)
    {
        return await _repo.GetForUserAsync(req.UserId, req.Take, ct);
    }
}