using System.Data;
using Dapper;
using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Data;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;

namespace MedHub.Application.Student.Catalog.GetCatalogCourses;

internal sealed class GetCatalogCoursesQueryHandler
    : IQueryHandler<GetCatalogCoursesQuery, PagedResponse<CatalogCourseListItemResponse>>
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 50;

    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IUserContext _userContext;

    public GetCatalogCoursesQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IUserContext userContext)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _userContext = userContext;
    }

    public async Task<Result<PagedResponse<CatalogCourseListItemResponse>>> Handle(
        GetCatalogCoursesQuery request,
        CancellationToken cancellationToken)
    {
        int page = Math.Max(1, request.Page);
        int pageSize = request.PageSize <= 0
            ? DefaultPageSize
            : Math.Clamp(request.PageSize, 1, MaxPageSize);
        int offset = (page - 1) * pageSize;

        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        const string countSql = """
            SELECT COUNT(*)::int
            FROM courses AS c
            WHERE c.status = 'Published'
            """;

        int totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                countSql,
                cancellationToken: cancellationToken));

        const string itemsSql = """
            SELECT
                c.id AS Id,
                c.title AS Title,
                c.description AS Description,
                COUNT(DISTINCT l.id)::int AS LessonsCount,
                COUNT(DISTINCT l.id) FILTER (WHERE l.status = 'Published')::int AS PublishedLessonsCount,
                (COUNT(DISTINCT v.id) > 0) AS HasVideo,
                COUNT(DISTINCT cp.id)::int AS CheckpointsCount,
                c.created_at AS CreatedAt,
                COALESCE(e.status = 'Active', false) AS IsEnrolled,
                e.status AS EnrollmentStatus
            FROM courses AS c
            LEFT JOIN enrollments AS e
                ON e.course_id = c.id
               AND e.student_id = @StudentId
            LEFT JOIN lessons AS l ON l.course_id = c.id
            LEFT JOIN videos AS v
                ON v.id = l.video_id
               AND v.lesson_id = l.id
               AND l.status = 'Published'
               AND v.status = 'Ready'
            LEFT JOIN checkpoints AS cp
                ON cp.video_id = v.id
               AND cp.status = 'Published'
            WHERE c.status = 'Published'
            GROUP BY c.id, c.title, c.description, c.created_at, e.status
            ORDER BY c.created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var items = (await connection.QueryAsync<CatalogCourseListItemResponse>(
            new CommandDefinition(
                itemsSql,
                new
                {
                    StudentId = _userContext.UserId,
                    PageSize = pageSize,
                    Offset = offset
                },
                cancellationToken: cancellationToken))).ToList();

        int totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling((double)totalCount / pageSize);

        return Result.Success(
            new PagedResponse<CatalogCourseListItemResponse>(
                items,
                page,
                pageSize,
                totalCount,
                totalPages));
    }
}
