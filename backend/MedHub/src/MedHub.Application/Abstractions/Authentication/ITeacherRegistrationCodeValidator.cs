namespace MedHub.Application.Abstractions.Authentication;

public interface ITeacherRegistrationCodeValidator
{
    bool IsValid(string? code);
}
