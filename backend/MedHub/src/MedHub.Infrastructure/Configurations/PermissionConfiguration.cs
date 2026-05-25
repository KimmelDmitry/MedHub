using MedHub.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedHub.Infrastructure.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(permission => permission.Id);

        builder.HasData(
            Permission.UsersRead,

            Permission.CoursesRead,
            Permission.CoursesCreate,
            Permission.CoursesUpdate,
            Permission.CoursesDelete,
            Permission.CoursesPublish,
            Permission.CoursesArchive,

            Permission.LessonsRead,
            Permission.LessonsCreate,
            Permission.LessonsUpdate,
            Permission.LessonsDelete,
            Permission.LessonsPublish,
            Permission.LessonsArchive,

            Permission.CheckpointsRead,
            Permission.CheckpointsCreate,
            Permission.CheckpointsUpdate,
            Permission.CheckpointsDelete,
            Permission.CheckpointsPublish,
            Permission.CheckpointsArchive,

            Permission.QuestionsRead,
            Permission.QuestionsCreate,
            Permission.QuestionsUpdate,
            Permission.QuestionsDelete,

            Permission.AttemptsRead,
            Permission.AttemptsStart,
            Permission.AttemptsSubmit,
            Permission.AttemptsCancel,
            Permission.AttemptsEvaluate,

            Permission.EnrollmentsRead,
            Permission.EnrollmentsCreate,
            Permission.EnrollmentsCancel,

            Permission.MediaRead,
            Permission.MediaUpload,
            Permission.MediaDelete,
            Permission.MediaProcess
        );
    }
}
