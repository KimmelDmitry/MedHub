using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Enrollments;

public static class EnrollmentErrors
{
    public static readonly Error NotFound =
        new("Enrollment.NotFound", "Запись на курс не найдена");

    public static readonly Error Forbidden =
        new("Enrollment.Forbidden", "Нет доступа к записи на курс");

    public static readonly Error Required =
        new("Enrollment.Required", "Для доступа к курсу нужна активная запись");

    public static readonly Error AlreadyActive =
        new("Enrollment.AlreadyActive", "Студент уже записан на этот курс");

    public static readonly Error NotActive =
        new("Enrollment.NotActive", "Запись на курс не активна");

    public static readonly Error CourseNotPublished =
        new("Enrollment.CourseNotPublished", "Записаться можно только на опубликованный курс");

    public static readonly Error CannotComplete =
        new("Enrollment.CannotComplete", "Нельзя завершить эту запись на курс");

    public static readonly Error CannotCancel =
        new("Enrollment.CannotCancel", "Нельзя отменить эту запись на курс");

    public static readonly Error InvalidStudentId =
        new("Enrollment.InvalidStudentId", "Идентификатор студента не может быть пустым");

    public static readonly Error InvalidCourseId =
        new("Enrollment.InvalidCourseId", "Идентификатор курса не может быть пустым");
}
