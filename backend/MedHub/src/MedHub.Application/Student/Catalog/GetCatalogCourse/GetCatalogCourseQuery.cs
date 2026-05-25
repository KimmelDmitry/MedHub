using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Student.Catalog.GetCatalogCourse;

public sealed record GetCatalogCourseQuery(Guid CourseId)
    : IQuery<CatalogCourseResponse>;
