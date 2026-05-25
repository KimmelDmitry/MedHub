using MedHub.Application.Abstractions.Authentication;
using Microsoft.Extensions.Configuration;

namespace MedHub.Infrastructure.Authentication;

internal sealed class TeacherRegistrationCodeValidator : ITeacherRegistrationCodeValidator
{
    private readonly string? _expectedCode;

    public TeacherRegistrationCodeValidator(IConfiguration configuration)
    {
        _expectedCode = configuration["DemoAuth:TeacherRegistrationCode"];
    }

    public bool IsValid(string? code)
    {
        return !string.IsNullOrWhiteSpace(_expectedCode) &&
               string.Equals(_expectedCode, code, StringComparison.Ordinal);
    }
}
