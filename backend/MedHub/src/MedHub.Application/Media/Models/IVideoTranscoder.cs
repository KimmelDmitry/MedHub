namespace MedHub.Application.Media.Models;

public interface IVideoTranscoder
{
    Task TranscodeToHlsAsync(
        string inputPath,
        string outputDirectory,
        CancellationToken cancellationToken = default);
}