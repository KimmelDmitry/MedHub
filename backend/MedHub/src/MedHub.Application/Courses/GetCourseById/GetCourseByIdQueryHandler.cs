using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses;

namespace MedHub.Application.Courses.GetCourseById;

internal sealed class GetCourseByIdQueryHandler
    : IQueryHandler<GetCourseByIdQuery, CourseResponse>
{
    private readonly ICourseRepository _courseRepository;

    public GetCourseByIdQueryHandler(
        ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<Result<CourseResponse>> Handle(
        GetCourseByIdQuery query,
        CancellationToken cancellationToken)
    {
        Course? course = await _courseRepository.GetByIdAsync(
            query.CourseId,
            cancellationToken);

        if (course is null)
        {
            return Result.Failure<CourseResponse>(
                CourseErrors.NotFound);
        }

        var response = new CourseResponse(
            course.Id,
            course.Title.Value,
            course.Description.Value,
            course.Status.ToString(),
            course.CreatorId,
            course.CreatedAt,
            course.Lessons
                .OrderBy(x => x.OrderNumber.Value)
                .Select(x => new CourseLessonResponse(
                    x.Id,
                    x.Title.Value,
                    x.OrderNumber.Value,
                    x.Status.ToString(),
                    x.ContentType.ToString(),
                    x.VideoId))
                .ToList());

        return Result.Success(response);
    }
}