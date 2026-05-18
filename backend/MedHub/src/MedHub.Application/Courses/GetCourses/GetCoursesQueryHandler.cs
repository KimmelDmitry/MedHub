using System.Data;
using Dapper;
using MedHub.Application.Abstractions.Data;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses;

namespace MedHub.Application.Courses.GetCourses;

internal sealed class GetCoursesQueryHandler : IQueryHandler<GetCoursesQuery, IReadOnlyList<CourseResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetCoursesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<IReadOnlyList<CourseResponse>>> Handle(
        GetCoursesQuery request, 
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        // SQL запрос: выбираем курсы и считаем количество уроков через подзапрос или JOIN
        const string sql = """
                           SELECT 
                               c.id AS Id,
                               c.title AS Title,
                               c.description AS Description,
                               c.created_at AS CreatedAt,
                               COUNT(l.id) AS LessonsCount
                           FROM courses AS c
                           LEFT JOIN lessons AS l ON l.course_id = c.id
                           GROUP BY c.id, c.title, c.description, c.created_at
                           ORDER BY c.created_at DESC
                           """;

        var courses = await connection.QueryAsync<CourseResponse>(sql);

        return Result.Success<IReadOnlyList<CourseResponse>>(courses.ToList());
    }
}