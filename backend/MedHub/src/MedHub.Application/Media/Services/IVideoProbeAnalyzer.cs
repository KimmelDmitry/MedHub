using MedHub.Application.Media.Models;

namespace MedHub.Application.Media.Services;

public interface IVideoProbeAnalyzer
{
    Task<VideoProbeResult> AnalyzeAsync(string filePath, CancellationToken cancellationToken = default);
}