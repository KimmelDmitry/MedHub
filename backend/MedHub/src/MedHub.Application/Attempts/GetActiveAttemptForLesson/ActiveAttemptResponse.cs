namespace MedHub.Application.Attempts.GetActiveAttemptForLesson;

public sealed class ActiveAttemptResponse
{
    public Guid AttemptId { get; init; }

    public Guid LessonId { get; init; }

    public Guid StudentId { get; init; }

    public DateTime StartedAt { get; init; }

    public string Status { get; init; } = string.Empty;

    public Guid[] AnsweredQuestionIds { get; init; } = [];
}
