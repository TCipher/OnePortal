namespace OnePortal.Application.Abstractions;

public interface ITokenService
{
    string CreateToken(int userId, string email, string? fullName, string globalRoleCode);
}
