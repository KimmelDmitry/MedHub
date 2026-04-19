using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(
        string Email,
        string FirstName,
        string LastName,
        string Password) : ICommand<Guid>;