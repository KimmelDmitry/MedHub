using MedHub.Application.Abstractions.Authentication;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Checkpoints;

internal static class CheckpointAccess
{
    public static async Task<Result> EnsureCanManageVideoAsync(
        VideoMaterial video,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        Lesson? lesson = await lessonRepository.GetByIdAsync(video.LessonId, cancellationToken);

        if (lesson is null)
        {
            return Result.Failure(LessonErrors.NotFound);
        }

        Course? course = await courseRepository.GetByIdAsync(lesson.CourseId, cancellationToken);

        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        if (userContext.IsInRole("Admin") || course.CreatorId == userContext.UserId)
        {
            return Result.Success();
        }

        return Result.Failure(
            new Error(
                "Checkpoint.Forbidden",
                "Only the course author can manage checkpoints for this video"));
    }

    public static async Task<Result> EnsureCanManageCheckpointAsync(
        Domain.Checkpoints.Checkpoint checkpoint,
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        VideoMaterial? video = await videoRepository.GetByIdAsync(checkpoint.VideoId, cancellationToken);

        if (video is null)
        {
            return Result.Failure(VideoErrors.NotFound);
        }

        return await EnsureCanManageVideoAsync(
            video,
            lessonRepository,
            courseRepository,
            userContext,
            cancellationToken);
    }
}
