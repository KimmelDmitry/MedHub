namespace MedHub.Application.Media.Services;

public interface IVideoTranscoder
{
    Task TranscodeToHlsAsync(
        string inputPath,
        string outputDirectory,
        CancellationToken cancellationToken = default);
}