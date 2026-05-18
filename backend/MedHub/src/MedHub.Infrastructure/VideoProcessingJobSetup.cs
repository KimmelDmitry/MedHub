using MedHub.Application.Media.Options;
using MedHub.Infrastructure.BackgroundJobs.MediaProcessing;
using Microsoft.Extensions.Options;
using Quartz;

namespace MedHub.Infrastructure;

internal sealed class VideoProcessingJobSetup : IConfigureOptions<QuartzOptions>
{
    private readonly VideoProcessingOptions _options;

    public VideoProcessingJobSetup(IOptions<VideoProcessingOptions> options)
    {
        _options = options.Value;
    }

    public void Configure(QuartzOptions options)
    {
        var jobKey = new JobKey(nameof(VideoProcessingJob));

        options
            .AddJob<VideoProcessingJob>(builder => builder.WithIdentity(jobKey))
            .AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity($"{nameof(VideoProcessingJob)}-trigger")
                .WithSimpleSchedule(schedule => schedule
                    .WithInterval(_options.PollInterval)
                    .RepeatForever()));
    }
}