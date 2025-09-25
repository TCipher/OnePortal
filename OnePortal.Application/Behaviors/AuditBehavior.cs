using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities; // keep this if AuditLog is in Domain

namespace OnePortal.Application.Common.Behaviors;

public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _current;
    private readonly IHttpContextAccessor _http;

    public AuditBehavior(IAuditLogger audit, ICurrentUser current, IHttpContextAccessor http)
    {
        _audit = audit;
        _current = current;
        _http = http;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var isCommand = typeof(TRequest).Namespace?.Contains(".Commands") == true;
        if (!isCommand) return await next();

        var action = typeof(TRequest).Name; // e.g., CreateUserCommand
        var httpContext = _http.HttpContext;
        var corrId = httpContext?.TraceIdentifier;
        var ip = httpContext?.Connection?.RemoteIpAddress?.ToString();
        var ua = httpContext?.Request?.Headers["User-Agent"].ToString();

        var entry = new AuditLog
        {
            ActorUserId = _current.UserId,
            ActorEmail = httpContext?.User?.FindFirst("email")?.Value ?? httpContext?.User?.Identity?.Name,
            ActorRoleCode = httpContext?.User?.FindFirst("global_role")?.Value,
            Action = action.ToUpperInvariant(),
            EntityType = InferEntityType(action),
            Success = true,
            DetailsJson = Truncate(JsonSerializer.Serialize(request, JsonOpts), 8000),
            CorrelationId = corrId,
            IpAddress = ip,
            UserAgent = ua
        };

        try
        {
            var response = await next();

            SetOptionalIdsFromRequest(request, entry);
            await _audit.LogAsync(entry, ct);
            return response;
        }
        catch (Exception ex)
        {
            entry.Success = false;
            entry.Message = ex.Message;
            SetOptionalIdsFromRequest(request, entry);
            await _audit.LogAsync(entry, ct);
            throw;
        }
    }

    private static string InferEntityType(string action)
    {
        if (action.Contains("User", StringComparison.OrdinalIgnoreCase)) return "UserDetails";
        if (action.Contains("PortalAccess", StringComparison.OrdinalIgnoreCase)) return "UserPortalAccess";
        return "Unknown";
    }

    private static void SetOptionalIdsFromRequest(object? req, AuditLog log)
    {
        if (req == null) return;
        var t = req.GetType();

        var idProp = t.GetProperty("Id") ?? t.GetProperty("EntityId");
        if (idProp?.GetValue(req) is int idVal) log.EntityId = idVal;

        var userIdProp = t.GetProperty("UserId");
        if (userIdProp?.GetValue(req) is int uid) log.EntityId ??= uid;

        var portalIdProp = t.GetProperty("PortalId");
        if (portalIdProp?.GetValue(req) is int pid) log.PortalId = pid;
    }

    private static string Truncate(string s, int len) => s.Length <= len ? s : s[..len];
}
