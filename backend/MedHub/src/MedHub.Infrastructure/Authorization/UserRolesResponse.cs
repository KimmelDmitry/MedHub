using MedHub.Domain.Users;

namespace MedHub.Infrastructure.Authorization;

public class UserRolesResponse
{
    public Guid UserId { get; init; }

    public List<Role> Roles { get; init; } = [];
}