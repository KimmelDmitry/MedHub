using System.Data;
using Dapper;
using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Data;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Enrollments;

namespace MedHub.Application.Student.Lessons.GetStudentLessonRuntime;

internal sealed class GetStudentLessonRuntimeQueryHandler
    : IQueryHandler<GetStudentLessonRuntimeQuery, StudentLessonRuntimeResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IUserContext _userContext;

    public GetStudentLessonRuntimeQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IUserContext userContext)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _userContext = userContext;
    }

    public async Task<Result<StudentLessonRuntimeResponse>> Handle(
        GetStudentLessonRuntimeQuery request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.IsInRole("Student"))
        {
            return Result.Failure<StudentLessonRuntimeResponse>(
                new Error(
                    "StudentRuntime.Forbidden",
                    "Only students can open the student lesson runtime."));
        }

        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        const string lessonSql = """
            SELECT
                l.id AS LessonId,
                l.course_id AS CourseId,
                c.title AS CourseTitle,
                l.title AS LessonTitle,
                v.id AS VideoId,
                COALESCE(v.duration_seconds, 0) AS DurationSeconds,
                COALESCE(v.width, 0) AS Width,
                COALESCE(v.height, 0) AS Height,
                COALESCE(v.title, v.original_file_name, '') AS Title
            FROM lessons l
            INNER JOIN courses c ON c.id = l.course_id
            INNER JOIN videos v ON v.id = l.video_id AND v.lesson_id = l.id
            WHERE l.id = @LessonId
              AND l.status = 'Published'
              AND c.status = 'Published'
              AND v.status = 'Ready'
            LIMIT 1
            """;

        LessonRuntimeRow? lesson = await connection.QueryFirstOrDefaultAsync<LessonRuntimeRow>(
            new CommandDefinition(
                lessonSql,
                new { request.LessonId },
                cancellationToken: cancellationToken));

        if (lesson is null)
        {
            return Result.Failure<StudentLessonRuntimeResponse>(
                new Error(
                    "StudentRuntime.NotFound",
                    "Published lesson runtime was not found."));
        }

        const string enrollmentSql = """
            SELECT EXISTS (
                SELECT 1
                FROM enrollments e
                WHERE e.student_id = @StudentId
                  AND e.course_id = @CourseId
                  AND e.status = 'Active'
            )
            """;

        bool isEnrolled = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                enrollmentSql,
                new
                {
                    StudentId = _userContext.UserId,
                    lesson.CourseId
                },
                cancellationToken: cancellationToken));

        if (!isEnrolled)
        {
            return Result.Failure<StudentLessonRuntimeResponse>(
                EnrollmentErrors.Required);
        }

        const string checkpointsSql = """
            SELECT
                id AS CheckpointId,
                timestamp_seconds AS TimestampSeconds,
                order_number AS OrderNumber,
                title AS Title,
                is_required AS IsRequired,
                is_graded AS IsGraded
            FROM checkpoints
            WHERE video_id = @VideoId
              AND status = 'Published'
            ORDER BY timestamp_seconds, order_number
            """;

        var checkpointRows = (await connection.QueryAsync<CheckpointRuntimeRow>(
            new CommandDefinition(
                checkpointsSql,
                new { lesson.VideoId },
                cancellationToken: cancellationToken))).ToList();

        Dictionary<Guid, List<QuestionRuntimeRow>> questionsByCheckpoint = new();
        Dictionary<Guid, List<AnswerOptionRuntimeRow>> optionsByQuestion = new();

        Guid[] checkpointIds = checkpointRows
            .Select(x => x.CheckpointId)
            .ToArray();

        if (checkpointIds.Length > 0)
        {
            const string questionsSql = """
                SELECT
                    id AS QuestionId,
                    checkpoint_id AS CheckpointId,
                    text AS Text,
                    type AS Type,
                    allow_retry AS AllowRetry,
                    time_limit_seconds AS TimeLimitSeconds,
                    reveal_correct_answer AS RevealCorrectAnswer
                FROM checkpoint_questions
                WHERE checkpoint_id = ANY(@CheckpointIds)
                  AND type = 'SingleChoice'
                ORDER BY id
                """;

            List<QuestionRuntimeRow> questionRows = (await connection.QueryAsync<QuestionRuntimeRow>(
                new CommandDefinition(
                    questionsSql,
                    new { CheckpointIds = checkpointIds },
                    cancellationToken: cancellationToken))).ToList();

            questionsByCheckpoint = questionRows
                .GroupBy(x => x.CheckpointId)
                .ToDictionary(x => x.Key, x => x.ToList());

            Guid[] questionIds = questionRows
                .Select(x => x.QuestionId)
                .ToArray();

            if (questionIds.Length > 0)
            {
                const string optionsSql = """
                    SELECT
                        id AS Id,
                        question_id AS QuestionId,
                        text AS Text
                    FROM checkpoint_answer_options
                    WHERE question_id = ANY(@QuestionIds)
                    ORDER BY id
                    """;

                List<AnswerOptionRuntimeRow> optionRows = (await connection.QueryAsync<AnswerOptionRuntimeRow>(
                    new CommandDefinition(
                        optionsSql,
                        new { QuestionIds = questionIds },
                        cancellationToken: cancellationToken))).ToList();

                optionsByQuestion = optionRows
                    .GroupBy(x => x.QuestionId)
                    .ToDictionary(x => x.Key, x => x.ToList());
            }
        }

        StudentRuntimeActiveAttemptResponse? activeAttempt =
            await GetActiveAttemptAsync(connection, request.LessonId, cancellationToken);

        List<StudentRuntimeCheckpointResponse> checkpoints = checkpointRows
            .Select(checkpoint =>
            {
                questionsByCheckpoint.TryGetValue(
                    checkpoint.CheckpointId,
                    out List<QuestionRuntimeRow>? questions);

                List<StudentRuntimeQuestionResponse> questionResponses = (questions ?? [])
                    .Select(question =>
                    {
                        optionsByQuestion.TryGetValue(
                            question.QuestionId,
                            out List<AnswerOptionRuntimeRow>? options);

                        return new StudentRuntimeQuestionResponse(
                            question.QuestionId,
                            question.Text,
                            question.Type,
                            question.AllowRetry,
                            question.TimeLimitSeconds,
                            question.RevealCorrectAnswer,
                            (options ?? [])
                                .Select(option => new StudentRuntimeAnswerOptionResponse(
                                    option.Id,
                                    option.Text))
                                .ToList());
                    })
                    .ToList();

                return new StudentRuntimeCheckpointResponse(
                    checkpoint.CheckpointId,
                    checkpoint.TimestampSeconds,
                    checkpoint.OrderNumber,
                    checkpoint.Title,
                    checkpoint.IsRequired,
                    checkpoint.IsGraded,
                    questionResponses);
            })
            .ToList();

        return Result.Success(
            new StudentLessonRuntimeResponse(
                lesson.LessonId,
                lesson.CourseId,
                lesson.CourseTitle,
                lesson.LessonTitle,
                new StudentRuntimeVideoResponse(
                    lesson.VideoId,
                    $"/api/v1/media/videos/{lesson.VideoId}/hls/master.m3u8",
                    lesson.DurationSeconds,
                    lesson.Width,
                    lesson.Height,
                    lesson.Title),
                checkpoints,
                activeAttempt));
    }

    private async Task<StudentRuntimeActiveAttemptResponse?> GetActiveAttemptAsync(
        IDbConnection connection,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                a.id AS AttemptId,
                a.status AS Status,
                COALESCE(
                    array_agg(aa.question_id) FILTER (WHERE aa.question_id IS NOT NULL),
                    ARRAY[]::uuid[]
                ) AS AnsweredQuestionIds
            FROM attempts a
            LEFT JOIN attempt_answers aa ON aa.attempt_id = a.id
            WHERE a.lesson_id = @LessonId
              AND a.student_id = @StudentId
              AND a.status IN ('InProgress', 'Completed')
            GROUP BY a.id, a.status
            ORDER BY
                CASE WHEN a.status = 'InProgress' THEN 0 ELSE 1 END,
                MAX(a.started_at) DESC
            LIMIT 1
            """;

        ActiveAttemptRuntimeRow? activeAttempt =
            await connection.QueryFirstOrDefaultAsync<ActiveAttemptRuntimeRow>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        LessonId = lessonId,
                        StudentId = _userContext.UserId
                    },
                    cancellationToken: cancellationToken));

        return activeAttempt is null
            ? null
            : new StudentRuntimeActiveAttemptResponse(
                activeAttempt.AttemptId,
                activeAttempt.Status,
                activeAttempt.AnsweredQuestionIds);
    }

    private sealed class LessonRuntimeRow
    {
        public Guid LessonId { get; init; }
        public Guid CourseId { get; init; }
        public string CourseTitle { get; init; } = string.Empty;
        public string LessonTitle { get; init; } = string.Empty;
        public Guid VideoId { get; init; }
        public int DurationSeconds { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }
        public string Title { get; init; } = string.Empty;
    }

    private sealed class CheckpointRuntimeRow
    {
        public Guid CheckpointId { get; init; }
        public int TimestampSeconds { get; init; }
        public int OrderNumber { get; init; }
        public string? Title { get; init; }
        public bool IsRequired { get; init; }
        public bool IsGraded { get; init; }
    }

    private sealed class QuestionRuntimeRow
    {
        public Guid QuestionId { get; init; }
        public Guid CheckpointId { get; init; }
        public string Text { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public bool AllowRetry { get; init; }
        public int? TimeLimitSeconds { get; init; }
        public bool RevealCorrectAnswer { get; init; }
    }

    private sealed class AnswerOptionRuntimeRow
    {
        public Guid Id { get; init; }
        public Guid QuestionId { get; init; }
        public string Text { get; init; } = string.Empty;
    }

    private sealed class ActiveAttemptRuntimeRow
    {
        public Guid AttemptId { get; init; }
        public string Status { get; init; } = string.Empty;
        public Guid[] AnsweredQuestionIds { get; init; } = [];
    }
}
