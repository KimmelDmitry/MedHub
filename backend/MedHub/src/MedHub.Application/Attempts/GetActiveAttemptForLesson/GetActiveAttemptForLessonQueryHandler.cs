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
                id AS AttemptId,
                lesson_id AS LessonId,
                user_id AS UserId,
                started_at_utc AS StartedAtUtc,
                status AS Status,
                current_question_index AS CurrentQuestionIndex
            FROM attempts
            WHERE lesson_id = @LessonId
              AND user_id = @UserId
              AND status = @Status
            LIMIT 1
            """;

        ActiveAttemptResponse? attempt =
            await connection.QueryFirstOrDefaultAsync<ActiveAttemptResponse>(
                sql,
                new
                {
                    request.LessonId,
                    UserId = _userContext.UserId,
                    Status = (int)AttemptStatus.InProgress
                });

        if (attempt is null)
        {
            return Result.Failure<ActiveAttemptResponse>(
                AttemptErrors.NotFound);
        }

        return attempt;
    }
}