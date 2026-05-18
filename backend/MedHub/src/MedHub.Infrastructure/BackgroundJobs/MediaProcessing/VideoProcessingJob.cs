using MedHub.Application.Abstractions.Media;
using Quartz;

namespace MedHub.Infrastructure.BackgroundJobs.MediaProcessing;

[DisallowConcurrentExecution]
public sealed class VideoProcessingJob : IJob
{
    private readonly IVideoProcessingService _processingService;

    public VideoProcessingJob(IVideoProcessingService processingService)
    {
        _processingService = processingService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _processingService.ProcessNextUploadedAsync(context.CancellationToken);
    }
}