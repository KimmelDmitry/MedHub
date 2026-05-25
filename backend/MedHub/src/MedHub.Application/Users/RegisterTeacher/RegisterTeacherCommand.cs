using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Users.RegisterTeacher;

public sealed record RegisterTeacherCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string TeacherRegistrationCode) : ICommand<Guid>;
