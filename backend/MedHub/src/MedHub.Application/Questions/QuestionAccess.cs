using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Checkpoints;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Questions;

internal static class QuestionAccess
{
    public static async Task<Result> EnsureCanManageCheckpointAsync(
        Checkpoint checkpoint,
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        return await CheckpointAccess.EnsureCanManageCheckpointAsync(
            checkpoint,
            videoRepository,
            lessonRepository,
            courseRepository,
            userContext,
            cancellationToken);
    }

    public static async Task<Result> EnsureCanManageQuestionAsync(
        Question question,
        ICheckpointRepository checkpointRepository,
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        Checkpoint? checkpoint = await checkpointRepository.GetByIdAsync(
            question.CheckpointId,
            cancellationToken);

        if (checkpoint is null)
        {
            return Result.Failure(CheckpointErrors.NotFound);
        }

        return await EnsureCanManageCheckpointAsync(
            checkpoint,
            videoRepository,
            lessonRepository,
            courseRepository,
            userContext,
            cancellationToken);
    }
}
