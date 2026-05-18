using MediatR;

namespace MedHub.Application.Media.Contracts;

public sealed record LinkVideoToLessonCommand(
    Guid LessonId,
    Guid VideoId
) : IRequest<Unit>;