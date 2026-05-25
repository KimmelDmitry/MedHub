using System.Data;
using Dapper;
using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Data;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Attempts;
using MedHub.Domain.Checkpoints.Enums;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Attempts.GetActiveAttemptForLesson;

internal sealed class GetActiveAttemptForLessonQueryHandler
    : IQueryHandler<GetActiveAttemptForLessonQuery, ActiveAttemptResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IUserContext _userContext;

    public GetActiveAttemptForLessonQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IUserContext userContext)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _userContext = userContext;
    }

    public async Task<Result<ActiveAttemptResponse>> Handle(
        GetActiveAttemptForLessonQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                a.id AS AttemptId,
                a.lesson_id AS LessonId,
                a.student_id AS StudentId,
                a.started_at AS StartedAt,
                a.status AS Status,
                COALESCE(
                    array_agg(aa.question_id) FILTER (WHERE aa.question_id IS NOT NULL),
                    ARRAY[]::uuid[]
                ) AS AnsweredQuestionIds
            FROM attempts a
            LEFT JOIN attempt_answers aa ON aa.attempt_id = a.id
            WHERE a.lesson_id = @LessonId
              AND a.student_id = @StudentId
              AND a.status = @Status
              AND EXISTS (
                  SELECT 1
                  FROM lessons l
                  INNER JOIN courses c ON c.id = l.course_id
                  INNER JOIN enrollments e
                      ON e.course_id = c.id
                     AND e.student_id = a.student_id
                     AND e.status = 'Active'
                  WHERE l.id = a.lesson_id
                    AND l.status = 'Published'
                    AND c.status = 'Published'
              )
            GROUP BY a.id, a.lesson_id, a.student_id, a.started_at, a.status
            LIMIT 1
            """;

        ActiveAttemptResponse? attempt =
            await connection.QueryFirstOrDefaultAsync<ActiveAttemptResponse>(
                sql,
                new
                {
                    request.LessonId,
                    StudentId = _userContext.UserId,
                    Status = AttemptStatus.InProgress.ToString()
                });

        if (attempt is null)
        {
            return Result.Failure<ActiveAttemptResponse>(
                AttemptErrors.NotFound);
        }

        return attempt;
    }
}
