using System.Data;
using Dapper;
using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Data;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;

namespace MedHub.Application.Student.Enrollments.GetMyEnrollments;

internal sealed class GetMyEnrollmentsQueryHandler
    : IQueryHandler<GetMyEnrollmentsQuery, IReadOnlyList<MyEnrollmentResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IUserContext _userContext;

    public GetMyEnrollmentsQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IUserContext userContext)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _userContext = userContext;
    }

    public async Task<Result<IReadOnlyList<MyEnrollmentResponse>>> Handle(
        GetMyEnrollmentsQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                e.id AS EnrollmentId,
                c.id AS CourseId,
                c.title AS CourseTitle,
                c.description AS CourseDescription,
                e.status AS Status,
                e.enrolled_at_utc AS EnrolledAtUtc,
                e.completed_at_utc AS CompletedAtUtc,
                COUNT(DISTINCT l.id) FILTER (WHERE l.status = 'Published')::int AS LessonsCount,
                0::int AS CompletedLessonsCount,
                MAX(a.started_at) AS LastAttemptAt
            FROM enrollments e
            INNER JOIN courses c ON c.id = e.course_id
            LEFT JOIN lessons l ON l.course_id = c.id
            LEFT JOIN attempts a
                ON a.lesson_id = l.id
               AND a.student_id = e.student_id
            WHERE e.student_id = @StudentId
            GROUP BY
                e.id,
                c.id,
                c.title,
                c.description,
                e.status,
                e.enrolled_at_utc,
                e.completed_at_utc
            ORDER BY e.enrolled_at_utc DESC
            """;

        var rows = (await connection.QueryAsync<MyEnrollmentResponse>(
            new CommandDefinition(
                sql,
                new { StudentId = _userContext.UserId },
                cancellationToken: cancellationToken))).ToList();

        return Result.Success<IReadOnlyList<MyEnrollmentResponse>>(rows);
    }
}
