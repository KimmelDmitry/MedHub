using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Clock;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Attempts;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Lessons;

namespace MedHub.Application.Attempts.StartAttempt;

internal sealed class StartAttemptCommandHandler
    : ICommandHandler<StartAttemptCommand, StartAttemptResponse>
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider  _dateTimeProvider;

    public StartAttemptCommandHandler(
        IAttemptRepository attemptRepository,
        ILessonRepository lessonRepository,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider)
    {
        _attemptRepository = attemptRepository;
        _lessonRepository = lessonRepository;
        _userContext = userContext;
        _dateTimeProvider = dateTimeProvider;   
    }

    public async Task<Result<StartAttemptResponse>> Handle(
        StartAttemptCommand command,
        CancellationToken cancellationToken)
    {
        Lesson? lesson = await _lessonRepository.GetByIdAsync(
            command.LessonId,
            cancellationToken);

        if (lesson is null)
            return Result.Failure<StartAttemptResponse>(LessonErrors.NotFound);

        Guid studentId = _userContext.UserId;

        Attempt? existingAttempt =
            await _attemptRepository.GetActiveAttemptAsync(
                studentId,
                command.LessonId,
                cancellationToken);

        if (existingAttempt is not null)
        {
            return Result.Success(
                new StartAttemptResponse(
                    existingAttempt.Id,
                    existingAttempt.LessonId,
                    existingAttempt.StartedAt,
                    existingAttempt.Status.ToString()));
        }

        DateTime utcNow = _dateTimeProvider.UtcNow;

        Result<Attempt> attemptResult = Attempt.Start(studentId, command.LessonId, utcNow);

        if (attemptResult.IsFailure)
        {
            return  Result.Failure<StartAttemptResponse>(attemptResult.Error);
        }

        var attempt = attemptResult.Value;
        
        _attemptRepository.Add(attempt);

        return Result.Success(
            new StartAttemptResponse(
                attempt.Id,
                attempt.LessonId,
                attempt.StartedAt,
                attempt.Status.ToString()));
    }
}