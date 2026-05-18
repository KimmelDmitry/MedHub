using FluentValidation;

namespace MedHub.Application.Courses.ArchiveCourse;

internal sealed class ArchiveCourseCommandValidator
    : AbstractValidator<ArchiveCourseCommand>
{
    public ArchiveCourseCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();
    }
}