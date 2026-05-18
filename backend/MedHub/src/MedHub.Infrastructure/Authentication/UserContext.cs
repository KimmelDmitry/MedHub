using System.Security.Claims;
using MedHub.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace MedHub.Infrastructure.Authentication;

internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId =>
        _httpContextAccessor
            .HttpContext?
            .User
            .GetUserId() ??
        throw new ApplicationException("User context is unavailable");

    public string IdentityId =>
        _httpContextAccessor
            .HttpContext?
            .User
            .GetIdentityId() ??
        throw new ApplicationException("User context is unavailable");
    
    
    public bool IsInRole(string roleName)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return false;

        // Проверка стандартных ролей ASP.NET
        if (user.IsInRole(roleName)) return true;

        // Проверка кастомных claims (если Keycloak кладет роли иначе)
        // Например, ищем claim с типом "realm_access" и парсим JSON, 
        // НО проще всего, если ты настроил CustomClaimsTransformation так, 
        // чтобы роли попадали в стандартные ClaimTypes.Role.
        
        return user.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == roleName);
    }
}