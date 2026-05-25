namespace MedHub.Api.Controllers;

internal static class Permissions
{
    // =========================
    // USERS
    // =========================

    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";
    public const string UsersDelete = "users:delete";

    // =========================
    // COURSES
    // =========================

    public const string CoursesRead = "courses:read";
    public const string CoursesCreate = "courses:create";
    public const string CoursesUpdate = "courses:update";
    public const string CoursesPublish = "courses:publish";
    public const string CoursesArchive = "courses:archive";

    // =========================
    // LESSONS
    // =========================

    public const string LessonsRead = "lessons:read";
    public const string LessonsCreate = "lessons:create";
    public const string LessonsUpdate = "lessons:update";
    public const string LessonsPublish = "lessons:publish";
    public const string LessonsArchive = "lessons:archive";
    
    // =========================
    // CHECKPOINTS
    // =========================
    
    public const string CheckpointsRead = "checkpoints:read";
    public const string CheckpointsCreate = "checkpoints:create";
    public const string CheckpointsUpdate = "checkpoints:update";
    public const string CheckpointsDelete = "checkpoints:delete";
    public const string CheckpointsPublish = "checkpoints:publish";
    public const string CheckpointsArchive = "checkpoints:archive";
    public const string CheckpointsWrite = "checkpoints:write";
    
    // =========================
    // QUESTIONS
    // =========================
    
    public const string QuestionsRead = "questions:read";
    public const string QuestionsCreate = "questions:create";
    public const string QuestionsUpdate = "questions:update";
    public const string QuestionsDelete = "questions:delete";
    
    // =========================
    // ATTEMPTS
    // =========================

    public const string AttemptsRead = "attempts:read";
    public const string AttemptsWrite = "attempts:write";
    public const string AttemptsStart = "attempts:start";
    public const string AttemptsSubmit = "attempts:submit";
    public const string AttemptsCancel = "attempts:cancel";
    public const string AttemptsEvaluate = "attempts:evaluate";

    // =========================
    // ENROLLMENTS
    // =========================

    public const string EnrollmentsRead = "enrollments:read";
    public const string EnrollmentsCreate = "enrollments:create";
    public const string EnrollmentsCancel = "enrollments:cancel";

    // =========================
    // MEDIA / VIDEO
    // =========================

    public const string MediaRead = "media:read";

    /// <summary>
    /// Загрузка видео / multipart upload
    /// </summary>
    public const string MediaUpload = "media:upload";

    /// <summary>
    /// Удаление видео
    /// </summary>
    public const string MediaDelete = "media:delete";

    /// <summary>
    /// Повторная обработка / модерация / ручной retry
    /// </summary>
    public const string MediaModerate = "media:moderate";

    // =========================
    // COMMENTS
    // =========================

    public const string CommentsRead = "comments:read";
    public const string CommentsWrite = "comments:write";
    public const string CommentsDelete = "comments:delete";

    // =========================
    // ADMIN
    // =========================

    public const string AdminPanel = "admin:panel";
    public const string SystemManage = "system:manage";
}
