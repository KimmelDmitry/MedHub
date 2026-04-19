using Microsoft.AspNetCore.Authorization;

namespace MedHub.Infrastructure.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
        : base(permission)
    {
    }
}