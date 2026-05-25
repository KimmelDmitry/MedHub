using System.Data;
using Dapper;
using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Data;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;

namespace MedHub.Application.Student.Dashboard.GetStudentDashboard;

internal sealed class GetStudentDashboardQueryHandler
    : IQueryHandler<GetStudentDashboardQuery, StudentDashboardResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IUserContext _userContext;

    public GetStudentDashboardQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IUserContext userContext)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _userContext = userContext;
    }

    public async Task<Result<StudentDashboardResponse>> Handle(
        GetStudentDashboardQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        IReadOnlyList<CourseRow> courseRows = await GetCourseRowsAsync(
            connection,
            cancellationToken);

        IReadOnlyList<StudentRecentAttemptResponse> recentAttempts = await GetRecentAttemptsAsync(
            connection,
            cancellationToken);

        var inProgressByCourse = (await GetInProgressAttemptsAsync(
                connection,
                cancellationToken))
            .ToDictionary(x => x.CourseId);

        var firstUncompletedByCourse = (await GetFirstUncompletedLessonsAsync(
                connection,
                cancellationToken))
            .ToDictionary(x => x.CourseId);

        var lastCompletedByCourse = (await GetLastCompletedAttemptsAsync(
                connection,
                cancellationToken))
            .ToDictionary(x => x.CourseId);

        var courses = courseRows
            .Select(course =>
            {
                StudentContinueLessonResponse? continueLesson =
                    inProgressByCourse.TryGetValue(course.CourseId, out ContinueLessonRow? inProgress)
                        ? ToContinueLesson(inProgress)
                        : firstUncompletedByCourse.TryGetValue(course.CourseId, out ContinueLessonRow? firstUncompleted)
                            ? ToContinueLesson(firstUncompleted)
                            : null;

                lastCompletedByCourse.TryGetValue(
                    course.CourseId,
                    out StudentRecentAttemptResponse? lastCompletedAttempt);

                int progressPercent = course.PublishedLessonsCount == 0
                    ? 0
                    : (int)Math.Round(
                        (double)course.CompletedLessonsCount * 100 / course.PublishedLessonsCount,
                        MidpointRounding.AwayFromZero);

                return new StudentDashboardCourseResponse(
                    course.EnrollmentId,
                    course.CourseId,
                    course.CourseTitle,
                    course.CourseDescription,
                    course.EnrollmentStatus,
                    course.EnrolledAtUtc,
                    course.PublishedLessonsCount,
                    course.CompletedLessonsCount,
                    Math.Clamp(progressPercent, 0, 100),
                    course.LastActivityAtUtc,
                    continueLesson,
                    lastCompletedAttempt);
            })
            .ToList();

        return Result.Success(
            new StudentDashboardResponse(
                courses,
                recentAttempts));
    }

    private async Task<IReadOnlyList<CourseRow>> GetCourseRowsAsync(
        IDbConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql = """
            WITH published_lessons AS (
                SELECT id, course_id
                FROM lessons
                WHERE status = 'Published'
            ),
            completed_lessons AS (
                SELECT DISTINCT a.lesson_id, l.course_id
                FROM attempts a
                INNER JOIN lessons l ON l.id = a.lesson_id
                WHERE a.student_id = @StudentId
                  AND a.status = 'Completed'
                  AND l.status = 'Published'
            )
            SELECT
                e.id AS EnrollmentId,
                c.id AS CourseId,
                c.title AS CourseTitle,
                c.description AS CourseDescription,
                e.status AS EnrollmentStatus,
                e.enrolled_at_utc AS EnrolledAtUtc,
                COUNT(DISTINCT pl.id)::int AS PublishedLessonsCount,
                COUNT(DISTINCT cl.lesson_id)::int AS CompletedLessonsCount,
                COALESCE(
                    MAX(COALESCE(a.updated_at, a.completed_at, a.started_at)),
                    e.enrolled_at_utc
                ) AS LastActivityAtUtc
            FROM enrollments e
            INNER JOIN courses c ON c.id = e.course_id
            LEFT JOIN published_lessons pl ON pl.course_id = c.id
            LEFT JOIN completed_lessons cl ON cl.course_id = c.id
            LEFT JOIN lessons attempt_lesson ON attempt_lesson.course_id = c.id
            LEFT JOIN attempts a
                ON a.lesson_id = attempt_lesson.id
               AND a.student_id = e.student_id
            WHERE e.student_id = @StudentId
              AND e.status = 'Active'
              AND c.status = 'Published'
            GROUP BY
                e.id,
                c.id,
                c.title,
                c.description,
                e.status,
                e.enrolled_at_utc
            ORDER BY LastActivityAtUtc DESC, e.enrolled_at_utc DESC
            """;

        return (await connection.QueryAsync<CourseRow>(
            new CommandDefinition(
                sql,
                new { StudentId = _userContext.UserId },
                cancellationToken: cancellationToken))).ToList();
    }

    private async Task<IReadOnlyList<StudentRecentAttemptResponse>> GetRecentAttemptsAsync(
        IDbConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                a.id AS AttemptId,
                c.id AS CourseId,
                c.title AS CourseTitle,
                l.id AS LessonId,
                l.title AS LessonTitle,
                a.status AS Status,
                a.score AS Score,
                a.started_at AS StartedAtUtc,
                a.completed_at AS CompletedAtUtc,
                COALESCE(a.updated_at, a.completed_at, a.started_at) AS UpdatedAtUtc
            FROM attempts a
            INNER JOIN lessons l ON l.id = a.lesson_id
            INNER JOIN courses c ON c.id = l.course_id
            INNER JOIN enrollments e
                ON e.course_id = c.id
               AND e.student_id = a.student_id
               AND e.status = 'Active'
            WHERE a.student_id = @StudentId
              AND c.status = 'Published'
              AND l.status = 'Published'
            ORDER BY COALESCE(a.updated_at, a.completed_at, a.started_at) DESC
            LIMIT 5
            """;

        return (await connection.QueryAsync<StudentRecentAttemptResponse>(
            new CommandDefinition(
                sql,
                new { StudentId = _userContext.UserId },
                cancellationToken: cancellationToken))).ToList();
    }

    private async Task<IReadOnlyList<ContinueLessonRow>> GetInProgressAttemptsAsync(
        IDbConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT ON (c.id)
                c.id AS CourseId,
                l.id AS LessonId,
                l.title AS LessonTitle,
                a.id AS AttemptId,
                a.status AS AttemptStatus,
                a.score AS Score,
                COALESCE(a.updated_at, a.started_at) AS UpdatedAtUtc
            FROM enrollments e
            INNER JOIN courses c ON c.id = e.course_id
            INNER JOIN lessons l ON l.course_id = c.id
            INNER JOIN attempts a
                ON a.lesson_id = l.id
               AND a.student_id = e.student_id
            WHERE e.student_id = @StudentId
              AND e.status = 'Active'
              AND c.status = 'Published'
              AND l.status = 'Published'
              AND a.status = 'InProgress'
            ORDER BY c.id, COALESCE(a.updated_at, a.started_at) DESC
            """;

        return (await connection.QueryAsync<ContinueLessonRow>(
            new CommandDefinition(
                sql,
                new { StudentId = _userContext.UserId },
                cancellationToken: cancellationToken))).ToList();
    }

    private async Task<IReadOnlyList<ContinueLessonRow>> GetFirstUncompletedLessonsAsync(
        IDbConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT ON (c.id)
                c.id AS CourseId,
                l.id AS LessonId,
                l.title AS LessonTitle,
                NULL::uuid AS AttemptId,
                NULL::text AS AttemptStatus,
                NULL::numeric AS Score,
                NULL::timestamp with time zone AS UpdatedAtUtc
            FROM enrollments e
            INNER JOIN courses c ON c.id = e.course_id
            INNER JOIN lessons l ON l.course_id = c.id
            WHERE e.student_id = @StudentId
              AND e.status = 'Active'
              AND c.status = 'Published'
              AND l.status = 'Published'
              AND NOT EXISTS (
                  SELECT 1
                  FROM attempts completed_attempt
                  WHERE completed_attempt.student_id = e.student_id
                    AND completed_attempt.lesson_id = l.id
                    AND completed_attempt.status = 'Completed'
              )
            ORDER BY c.id, l.order_number ASC, l.created_at ASC
            """;

        return (await connection.QueryAsync<ContinueLessonRow>(
            new CommandDefinition(
                sql,
                new { StudentId = _userContext.UserId },
                cancellationToken: cancellationToken))).ToList();
    }

    private async Task<IReadOnlyList<StudentRecentAttemptResponse>> GetLastCompletedAttemptsAsync(
        IDbConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT ON (c.id)
                a.id AS AttemptId,
                c.id AS CourseId,
                c.title AS CourseTitle,
                l.id AS LessonId,
                l.title AS LessonTitle,
                a.status AS Status,
                a.score AS Score,
                a.started_at AS StartedAtUtc,
                a.completed_at AS CompletedAtUtc,
                COALESCE(a.updated_at, a.completed_at, a.started_at) AS UpdatedAtUtc
            FROM attempts a
            INNER JOIN lessons l ON l.id = a.lesson_id
            INNER JOIN courses c ON c.id = l.course_id
            INNER JOIN enrollments e
                ON e.course_id = c.id
               AND e.student_id = a.student_id
               AND e.status = 'Active'
            WHERE a.student_id = @StudentId
              AND a.status = 'Completed'
              AND c.status = 'Published'
              AND l.status = 'Published'
            ORDER BY c.id, COALESCE(a.completed_at, a.updated_at, a.started_at) DESC
            """;

        return (await connection.QueryAsync<StudentRecentAttemptResponse>(
            new CommandDefinition(
                sql,
                new { StudentId = _userContext.UserId },
                cancellationToken: cancellationToken))).ToList();
    }

    private static StudentContinueLessonResponse ToContinueLesson(ContinueLessonRow row)
    {
        return new StudentContinueLessonResponse(
            row.LessonId,
            row.LessonTitle,
            row.AttemptId,
            row.AttemptStatus,
            row.Score,
            row.UpdatedAtUtc);
    }

    private sealed class CourseRow
    {
        public Guid EnrollmentId { get; init; }
        public Guid CourseId { get; init; }
        public string CourseTitle { get; init; } = string.Empty;
        public string? CourseDescription { get; init; }
        public string EnrollmentStatus { get; init; } = string.Empty;
        public DateTime EnrolledAtUtc { get; init; }
        public int PublishedLessonsCount { get; init; }
        public int CompletedLessonsCount { get; init; }
        public DateTime LastActivityAtUtc { get; init; }
    }

    private sealed class ContinueLessonRow
    {
        public Guid CourseId { get; init; }
        public Guid LessonId { get; init; }
        public string LessonTitle { get; init; } = string.Empty;
        public Guid? AttemptId { get; init; }
        public string? AttemptStatus { get; init; }
        public decimal? Score { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}
