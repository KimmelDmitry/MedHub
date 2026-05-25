using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Student.Catalog.GetCatalogCourses;

public sealed record GetCatalogCoursesQuery(int Page, int PageSize)
    : IQuery<PagedResponse<CatalogCourseListItemResponse>>;
