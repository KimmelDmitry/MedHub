using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Users.LogInUser;

public sealed record LogInUserCommand(string Email, string Password)
    : ICommand<AccessTokenResponse>;