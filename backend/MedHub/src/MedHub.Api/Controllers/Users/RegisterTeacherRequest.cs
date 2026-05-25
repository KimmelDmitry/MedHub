namespace MedHub.Api.Controllers.Users;

public sealed record RegisterTeacherRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string TeacherRegistrationCode);
