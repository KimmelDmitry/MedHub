using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Lessons;

namespace MedHub.Application.Lessons.GetLessonById;

internal sealed class GetLessonByIdQueryHandler
    : IQueryHandler<GetLessonByIdQuery, LessonResponse>
{
    private readonly ILessonRepository _lessonRepository;

    public GetLessonByIdQueryHandler(
        ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<Result<LessonResponse>> Handle(
        GetLessonByIdQuery query,
        CancellationToken cancellationToken)
    {
        Lesson? lesson = await _lessonRepository.GetByIdAsync(
            query.LessonId,
            cancellationToken);

        if (lesson is null)
        {
            return Result.Failure<LessonResponse>(
                LessonErrors.NotFound);
        }

        var response = new LessonResponse(
            lesson.Id,
            lesson.CourseId,
            lesson.Title.Value,
            lesson.OrderNumber.Value,
            lesson.ContentType.ToString(),
            lesson.ContentUrl,
            lesson.Status.ToString(),
            lesson.VideoId,
            lesson.CreatedAt);

        return Result.Success(response);
    }
}