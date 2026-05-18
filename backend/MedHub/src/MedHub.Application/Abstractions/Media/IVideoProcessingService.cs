namespace MedHub.Application.Abstractions.Media;


public interface IVideoProcessingService
{
    Task ProcessNextUploadedAsync(CancellationToken cancellationToken = default);
    Task ProcessAsync(Guid videoId, CancellationToken cancellationToken = default);
}
