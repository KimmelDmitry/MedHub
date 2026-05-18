using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Attempts.StartAttempt;

public sealed record StartAttemptCommand(
    Guid LessonId
) : ICommand<StartAttemptResponse>;