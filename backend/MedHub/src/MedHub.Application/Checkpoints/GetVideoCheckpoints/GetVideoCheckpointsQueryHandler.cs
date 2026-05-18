using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;

namespace MedHub.Application.Checkpoints.GetVideoCheckpoints;

internal sealed class GetVideoCheckpointsQueryHandler
    : IQueryHandler<GetVideoCheckpointsQuery, IReadOnlyList<VideoCheckpointResponse>>
{
    private readonly ICheckpointRepository _checkpointRepository;

    public GetVideoCheckpointsQueryHandler(ICheckpointRepository checkpointRepository)
    {
        _checkpointRepository = checkpointRepository;
    }

    public async Task<Result<IReadOnlyList<VideoCheckpointResponse>>> Handle(
        GetVideoCheckpointsQuery query,
        CancellationToken cancellationToken)
    {
        var checkpoints = await _checkpointRepository.GetByVideoIdAsync(
            query.VideoId,
            cancellationToken);

        var response = checkpoints
            .OrderBy(x => x.Timestamp.Value)
            .ThenBy(x => x.OrderNumber)
            .Select(x => new VideoCheckpointResponse(
                x.Id,
                x.Timestamp.Value,
                x.OrderNumber,
                x.Title,
                x.IsRequired,
                x.IsGraded,
                x.Status.ToString(),
                x.Questions.Count))
            .ToList();

        return Result.Success<IReadOnlyList<VideoCheckpointResponse>>(response);
    }
}