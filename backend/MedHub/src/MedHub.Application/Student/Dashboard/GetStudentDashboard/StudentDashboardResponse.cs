namespace MedHub.Application.Student.Dashboard.GetStudentDashboard;

public sealed record StudentDashboardResponse(
    IReadOnlyList<StudentDashboardCourseResponse> EnrolledCourses,
    IReadOnlyList<StudentRecentAttemptResponse> RecentAttempts);

public sealed record StudentDashboardCourseResponse(
    Guid EnrollmentId,
    Guid CourseId,
    string CourseTitle,
    string? CourseDescription,
    string EnrollmentStatus,
    DateTime EnrolledAtUtc,
    int PublishedLessonsCount,
    int CompletedLessonsCount,
    int ProgressPercent,
    DateTime LastActivityAtUtc,
    StudentContinueLessonResponse? ContinueLesson,
    StudentRecentAttemptResponse? LastCompletedAttempt);

public sealed record StudentContinueLessonResponse(
    Guid LessonId,
    string LessonTitle,
    Guid? AttemptId,
    string? AttemptStatus,
    decimal? Score,
    DateTime? UpdatedAtUtc);

public sealed record StudentRecentAttemptResponse(
    Guid AttemptId,
    Guid CourseId,
    string CourseTitle,
    Guid LessonId,
    string LessonTitle,
    string Status,
    decimal Score,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime UpdatedAtUtc);
