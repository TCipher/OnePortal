namespace OnePortal.Blazor.Models
{
    public record PortalAccessItem(
        int PortalId,
        string PortalCode,
        string PortalName,
        int PortalRoleId,
        string PortalRoleCode,
        string PortalRoleName,
        bool IsActive);

    public record PortalNavItem(
        string Code,
        string Name,
        string Url,
        string? IconSvg = null);
}
