using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Users.GetLoggedInUser;

public sealed record GetLoggedInUserQuery : IQuery<UserResponse>;