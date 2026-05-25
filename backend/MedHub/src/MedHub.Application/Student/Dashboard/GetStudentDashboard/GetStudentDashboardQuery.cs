using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Student.Dashboard.GetStudentDashboard;

public sealed record GetStudentDashboardQuery()
    : IQuery<StudentDashboardResponse>;
