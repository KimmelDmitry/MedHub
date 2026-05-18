using FluentValidation;

namespace MedHub.Application.Courses.GetCourseById;

internal sealed class GetCourseByIdQueryValidator
    : AbstractValidator<GetCourseByIdQuery>
{
    public GetCourseByIdQueryValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();
    }
}