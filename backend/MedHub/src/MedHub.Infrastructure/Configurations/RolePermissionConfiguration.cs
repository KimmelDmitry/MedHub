using MedHub.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedHub.Infrastructure.Configurations;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasKey(rolePermission =>
            new { rolePermission.RoleId, rolePermission.PermissionId });

        builder.HasData(
            // =========================================================
            // STUDENT
            // =========================================================

            // Users
            Create(Role.Student, Permission.UsersRead),

            // Courses
            Create(Role.Student, Permission.CoursesRead),

            // Lessons
            Create(Role.Student, Permission.LessonsRead),

            // Checkpoints
            Create(Role.Student, Permission.CheckpointsRead),

            // Questions
            Create(Role.Student, Permission.QuestionsRead),

            // Attempts
            Create(Role.Student, Permission.AttemptsRead),
            Create(Role.Student, Permission.AttemptsStart),
            Create(Role.Student, Permission.AttemptsSubmit),
            Create(Role.Student, Permission.AttemptsCancel),

            // Media
            Create(Role.Student, Permission.MediaRead),

            // =========================================================
            // TEACHER
            // =========================================================

            // Users
            Create(Role.Teacher, Permission.UsersRead),

            // Courses
            Create(Role.Teacher, Permission.CoursesRead),
            Create(Role.Teacher, Permission.CoursesCreate),
            Create(Role.Teacher, Permission.CoursesUpdate),
            Create(Role.Teacher, Permission.CoursesPublish),
            Create(Role.Teacher, Permission.CoursesArchive),

            // Lessons
            Create(Role.Teacher, Permission.LessonsRead),
            Create(Role.Teacher, Permission.LessonsCreate),
            Create(Role.Teacher, Permission.LessonsUpdate),
            Create(Role.Teacher, Permission.LessonsPublish),
            Create(Role.Teacher, Permission.LessonsArchive),

            // Checkpoints
            Create(Role.Teacher, Permission.CheckpointsRead),
            Create(Role.Teacher, Permission.CheckpointsCreate),
            Create(Role.Teacher, Permission.CheckpointsUpdate),
            Create(Role.Teacher, Permission.CheckpointsPublish),
            Create(Role.Teacher, Permission.CheckpointsArchive),

            // Questions
            Create(Role.Teacher, Permission.QuestionsRead),
            Create(Role.Teacher, Permission.QuestionsCreate),
            Create(Role.Teacher, Permission.QuestionsUpdate),
            Create(Role.Teacher, Permission.QuestionsDelete),

            // Attempts
            Create(Role.Teacher, Permission.AttemptsRead),
            Create(Role.Teacher, Permission.AttemptsEvaluate),

            // Media
            Create(Role.Teacher, Permission.MediaRead),
            Create(Role.Teacher, Permission.MediaUpload),
            Create(Role.Teacher, Permission.MediaDelete),
            Create(Role.Teacher, Permission.MediaProcess),

            // =========================================================
            // ADMIN
            // =========================================================

            // Users
            Create(Role.Admin, Permission.UsersRead),

            // Courses
            Create(Role.Admin, Permission.CoursesRead),
            Create(Role.Admin, Permission.CoursesCreate),
            Create(Role.Admin, Permission.CoursesUpdate),
            Create(Role.Admin, Permission.CoursesDelete),
            Create(Role.Admin, Permission.CoursesPublish),
            Create(Role.Admin, Permission.CoursesArchive),

            // Lessons
            Create(Role.Admin, Permission.LessonsRead),
            Create(Role.Admin, Permission.LessonsCreate),
            Create(Role.Admin, Permission.LessonsUpdate),
            Create(Role.Admin, Permission.LessonsDelete),
            Create(Role.Admin, Permission.LessonsPublish),
            Create(Role.Admin, Permission.LessonsArchive),

            // Checkpoints
            Create(Role.Admin, Permission.CheckpointsRead),
            Create(Role.Admin, Permission.CheckpointsCreate),
            Create(Role.Admin, Permission.CheckpointsUpdate),
            Create(Role.Admin, Permission.CheckpointsDelete),
            Create(Role.Admin, Permission.CheckpointsPublish),
            Create(Role.Admin, Permission.CheckpointsArchive),

            // Questions
            Create(Role.Admin, Permission.QuestionsRead),
            Create(Role.Admin, Permission.QuestionsCreate),
            Create(Role.Admin, Permission.QuestionsUpdate),
            Create(Role.Admin, Permission.QuestionsDelete),

            // Attempts
            Create(Role.Admin, Permission.AttemptsRead),
            Create(Role.Admin, Permission.AttemptsStart),
            Create(Role.Admin, Permission.AttemptsSubmit),
            Create(Role.Admin, Permission.AttemptsCancel),
            Create(Role.Admin, Permission.AttemptsEvaluate),

            // Media
            Create(Role.Admin, Permission.MediaRead),
            Create(Role.Admin, Permission.MediaUpload),
            Create(Role.Admin, Permission.MediaDelete),
            Create(Role.Admin, Permission.MediaProcess)
        );
    }

    private static RolePermission Create(Role role, Permission permission)
    {
        return new RolePermission
        {
            RoleId = role.Id,
            PermissionId = permission.Id
        };
    }
}