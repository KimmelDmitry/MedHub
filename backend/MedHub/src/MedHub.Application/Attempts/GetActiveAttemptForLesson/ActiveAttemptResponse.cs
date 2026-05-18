namespace MedHub.Application.Attempts.GetActiveAttemptForLesson;

public sealed class ActiveAttemptResponse
{
    public Guid AttemptId { get; init; }

    public Guid LessonId { get; init; }

    public Guid UserId { get; init; }

    public DateTime StartedAtUtc { get; init; }

    public int Status { get; init; }

    public int CurrentQuestionIndex { get; init; }
}