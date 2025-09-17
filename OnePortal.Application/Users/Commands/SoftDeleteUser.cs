using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.Users.Commands;

public record SoftDeleteUserCommand(int Id) : IRequest<bool>;

public class SoftDeleteUserValidator : AbstractValidator<SoftDeleteUserCommand>
{
    public SoftDeleteUserValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class SoftDeleteUserHandler : IRequestHandler<SoftDeleteUserCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public SoftDeleteUserHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(SoftDeleteUserCommand req, CancellationToken ct)
    {
        var user = await _uow.Users.GetWithAccessesAsync(req.Id, ct);
        if (user == null) return false;

        user.IsActive = false;
        user.Modifieddate = DateTime.UtcNow;

        foreach (var a in user.PortalAccesses)
        {
            a.IsActive = false;
            a.RevokedOn = DateTime.UtcNow;
        }

        await _uow.Users.UpdateAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return true;
    }
}

