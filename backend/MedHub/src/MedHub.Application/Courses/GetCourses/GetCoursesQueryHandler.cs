using System.Data;
using Dapper;
using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Data;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;

namespace MedHub.Application.Courses.GetCourses;

internal sealed class GetCoursesQueryHandler
    : IQueryHandler<GetCoursesQuery, IReadOnlyList<CourseListItemResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IUserContext _userContext;

    public GetCoursesQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IUserContext userContext)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _userContext = userContext;
    }

    public async Task<Result<IReadOnlyList<CourseListItemResponse>>> Handle(
        GetCoursesQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                c.id AS Id,
                c.title AS Title,
                c.description AS Description,
                c.status AS Status,
                c.created_at AS CreatedOnUtc,
                COUNT(l.id)::int AS LessonsCount
            FROM courses AS c
            LEFT JOIN lessons AS l ON l.course_id = c.id
            WHERE c.creator_id = @UserId
            GROUP BY c.id, c.title, c.description, c.status, c.created_at
            ORDER BY c.created_at DESC
            """;

        var courses = await connection.QueryAsync<CourseListItemResponse>(
            sql,
            new
            {
                _userContext.UserId
            });

        return Result.Success<IReadOnlyList<CourseListItemResponse>>(courses.ToList());
    }
}
