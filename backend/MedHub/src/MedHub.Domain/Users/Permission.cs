namespace MedHub.Domain.Users;

public sealed class Permission
{
    
    // Users
    public static readonly Permission UsersRead = new(1, "users:read");

    // Courses
    public static readonly Permission CoursesRead = new(2, "courses:read");

    public static readonly Permission CoursesCreate = new(3, "courses:create");

    public static readonly Permission CoursesUpdate = new(4, "courses:update");

    public static readonly Permission CoursesDelete = new(5, "courses:delete");

    public static readonly Permission CoursesPublish = new(6, "courses:publish");

    public static readonly Permission CoursesArchive = new(7, "courses:archive");

    // Lessons
    public static readonly Permission LessonsRead = new(8, "lessons:read");

    public static readonly Permission LessonsCreate = new(9, "lessons:create");

    public static readonly Permission LessonsUpdate = new(10, "lessons:update");

    public static readonly Permission LessonsDelete = new(11, "lessons:delete");

    public static readonly Permission LessonsPublish = new(12, "lessons:publish");

    public static readonly Permission LessonsArchive = new(13, "lessons:archive");

    // Checkpoints
    public static readonly Permission CheckpointsRead = new(14, "checkpoints:read");

    public static readonly Permission CheckpointsCreate = new(15, "checkpoints:create");

    public static readonly Permission CheckpointsUpdate = new(16, "checkpoints:update");

    public static readonly Permission CheckpointsDelete = new(17, "checkpoints:delete");

    public static readonly Permission CheckpointsPublish = new(18, "checkpoints:publish");

    public static readonly Permission CheckpointsArchive = new(19, "checkpoints:archive");

    // Questions
    public static readonly Permission QuestionsRead = new(20, "questions:read");

    public static readonly Permission QuestionsCreate = new(21, "questions:create");

    public static readonly Permission QuestionsUpdate = new(22, "questions:update");

    public static readonly Permission QuestionsDelete = new(23, "questions:delete");

    // Attempts
    public static readonly Permission AttemptsRead = new(24, "attempts:read");

    public static readonly Permission AttemptsStart = new(25, "attempts:start");

    public static readonly Permission AttemptsSubmit = new(26, "attempts:submit");

    public static readonly Permission AttemptsCancel = new(27, "attempts:cancel");

    public static readonly Permission AttemptsEvaluate = new(28, "attempts:evaluate");

    // Media / Video
    public static readonly Permission MediaRead = new(29, "media:read");

    public static readonly Permission MediaUpload = new(30, "media:upload");

    public static readonly Permission MediaDelete = new(31, "media:delete");

    public static readonly Permission MediaProcess = new(32, "media:process");

    private Permission(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; init; }

    public string Name { get; init; }
}
