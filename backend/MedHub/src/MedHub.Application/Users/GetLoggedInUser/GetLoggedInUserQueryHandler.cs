using MedHub.Domain.Abstractions;
using Dapper;
using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Data;
using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Users.GetLoggedInUser;

internal sealed class GetLoggedInUserQueryHandler
    : IQueryHandler<GetLoggedInUserQuery, UserResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IUserContext _userContext;

    public GetLoggedInUserQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IUserContext userContext)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _userContext = userContext;
    }

    public async Task<Result<UserResponse>> Handle(
        GetLoggedInUserQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                u.id AS Id,
                u.first_name AS FirstName,
                u.last_name AS LastName,
                u.email AS Email,
                r.name AS Role,
                p.name AS Permission
            FROM users u
            JOIN role_user ru ON ru.users_id = u.id
            JOIN roles r ON r.id = ru.roles_id
            LEFT JOIN role_permissions rp ON rp.role_id = r.id
            LEFT JOIN permissions p ON p.id = rp.permission_id
            WHERE u.identity_id = @IdentityId
            ORDER BY r.id DESC
            """;

        var rows = (await connection.QueryAsync<UserProfileRow>(
            sql,
            new
            {
                _userContext.IdentityId
            })).ToList();

        var user = rows.First();

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Permissions = rows
                .Select(row => row.Permission)
                .Where(permission => !string.IsNullOrWhiteSpace(permission))
                .Select(permission => permission!)
                .Distinct()
                .ToList()
        };
    }

    private sealed class UserProfileRow
    {
        public Guid Id { get; init; }

        public string Email { get; init; } = string.Empty;

        public string FirstName { get; init; } = string.Empty;

        public string LastName { get; init; } = string.Empty;

        public string Role { get; init; } = string.Empty;

        public string? Permission { get; init; }
    }
}
