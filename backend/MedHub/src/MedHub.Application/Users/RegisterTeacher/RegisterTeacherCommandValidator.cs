using FluentValidation;

namespace MedHub.Application.Users.RegisterTeacher;

internal sealed class RegisterTeacherCommandValidator : AbstractValidator<RegisterTeacherCommand>
{
    public RegisterTeacherCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty();

        RuleFor(c => c.LastName).NotEmpty();

        RuleFor(c => c.Email).EmailAddress();

        RuleFor(c => c.Password).NotEmpty().MinimumLength(5);

        RuleFor(c => c.TeacherRegistrationCode).NotEmpty();
    }
}
