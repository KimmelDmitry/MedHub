using System.Data;
using Dapper;
using MedHub.Application.Abstractions.Data;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Lessons;

namespace MedHub.Application.Lessons.GetLessonsByCourse;

internal sealed class GetLessonsByCourseQueryHandler : IQueryHandler<GetLessonsByCourseQuery, IReadOnlyList<LessonResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetLessonsByCourseQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<IReadOnlyList<LessonResponse>>> Handle(
        GetLessonsByCourseQuery request, 
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
                           SELECT 
                               l.id AS Id,
                               l.title AS Title,
                               l.order_number AS "Order",
                               l.content_type AS ContentType,
                               (v.id IS NOT NULL) AS HasVideo
                           FROM lessons AS l
                           LEFT JOIN videos AS v ON v.lesson_id = l.id
                           WHERE l.course_id = @CourseId
                           ORDER BY l.order_number ASC
                           """;

        var parameters = new { request.CourseId };

        var lessons = await connection.QueryAsync<LessonResponse>(sql, parameters);

        return Result.Success<IReadOnlyList<LessonResponse>>(lessons.ToList());
    }
}
