using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Media.GetVideoHlsFile;

public sealed record GetVideoHlsFileQuery(
    Guid VideoId,
    string FileName)
    : IQuery<VideoHlsFileResponse>;
