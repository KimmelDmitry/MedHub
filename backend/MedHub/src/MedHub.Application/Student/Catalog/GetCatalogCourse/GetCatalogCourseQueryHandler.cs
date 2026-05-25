using System.Data;
using Dapper;
using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Data;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;

namespace MedHub.Application.Student.Catalog.GetCatalogCourse;

internal sealed class GetCatalogCourseQueryHandler
    : IQueryHandler<GetCatalogCourseQuery, CatalogCourseResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IUserContext _userContext;

    public GetCatalogCourseQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IUserContext userContext)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _userContext = userContext;
    }

    public async Task<Result<CatalogCourseResponse>> Handle(
        GetCatalogCourseQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        const string courseSql = """
            SELECT
                c.id AS Id,
                c.title AS Title,
                c.description AS Description,
                COALESCE(e.status = 'Active', false) AS IsEnrolled,
                e.status AS EnrollmentStatus
            FROM courses AS c
            LEFT JOIN enrollments AS e
                ON e.course_id = c.id
               AND e.student_id = @StudentId
            WHERE c.id = @CourseId
              AND c.status = 'Published'
            LIMIT 1
            """;

        CourseRow? course = await connection.QueryFirstOrDefaultAsync<CourseRow>(
            new CommandDefinition(
                courseSql,
                new
                {
                    request.CourseId,
                    StudentId = _userContext.UserId
                },
                cancellationToken: cancellationToken));

        if (course is null)
        {
            return Result.Failure<CatalogCourseResponse>(
                new Error(
                    "Catalog.CourseNotFound",
                    "Published course was not found."));
        }

        const string lessonsSql = """
            SELECT
                l.id AS Id,
                l.title AS Title,
                l.order_number AS "Order",
                l.content_type AS ContentType,
                (l.video_id IS NOT NULL) AS HasVideo,
                (v.id IS NOT NULL AND v.status = 'Ready') AS VideoReady,
                v.duration_seconds AS DurationSeconds,
                COUNT(DISTINCT cp.id)::int AS CheckpointsCount
            FROM lessons AS l
            LEFT JOIN videos AS v
                ON v.id = l.video_id
               AND v.lesson_id = l.id
            LEFT JOIN checkpoints AS cp
                ON cp.video_id = v.id
               AND cp.status = 'Published'
            WHERE l.course_id = @CourseId
              AND l.status = 'Published'
            GROUP BY
                l.id,
                l.title,
                l.order_number,
                l.content_type,
                l.video_id,
                v.id,
                v.status,
                v.duration_seconds
            ORDER BY l.order_number ASC
            """;

        var lessons = (await connection.QueryAsync<CatalogLessonItemResponse>(
            new CommandDefinition(
                lessonsSql,
                new { request.CourseId },
                cancellationToken: cancellationToken))).ToList();

        return Result.Success(
            new CatalogCourseResponse(
                course.Id,
                course.Title,
                course.Description,
                course.IsEnrolled,
                course.EnrollmentStatus,
                lessons));
    }

    private sealed class CourseRow
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public bool IsEnrolled { get; init; }
        public string? EnrollmentStatus { get; init; }
    }
}
