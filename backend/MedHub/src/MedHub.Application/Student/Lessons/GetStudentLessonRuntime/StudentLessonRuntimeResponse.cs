namespace MedHub.Application.Student.Lessons.GetStudentLessonRuntime;

public sealed record StudentLessonRuntimeResponse(
    Guid LessonId,
    Guid CourseId,
    string CourseTitle,
    string LessonTitle,
    StudentRuntimeVideoResponse Video,
    IReadOnlyList<StudentRuntimeCheckpointResponse> Checkpoints,
    StudentRuntimeActiveAttemptResponse? ActiveAttempt);

public sealed record StudentRuntimeVideoResponse(
    Guid VideoId,
    string HlsMasterUrl,
    int DurationSeconds,
    int Width,
    int Height,
    string Title);

public sealed record StudentRuntimeCheckpointResponse(
    Guid CheckpointId,
    int TimestampSeconds,
    int OrderNumber,
    string? Title,
    bool IsRequired,
    bool IsGraded,
    IReadOnlyList<StudentRuntimeQuestionResponse> Questions);

public sealed record StudentRuntimeQuestionResponse(
    Guid QuestionId,
    string Text,
    string Type,
    bool AllowRetry,
    int? TimeLimitSeconds,
    bool RevealCorrectAnswer,
    IReadOnlyList<StudentRuntimeAnswerOptionResponse> AnswerOptions);

public sealed record StudentRuntimeAnswerOptionResponse(
    Guid Id,
    string Text);

public sealed record StudentRuntimeActiveAttemptResponse(
    Guid AttemptId,
    string Status,
    IReadOnlyList<Guid> AnsweredQuestionIds);
