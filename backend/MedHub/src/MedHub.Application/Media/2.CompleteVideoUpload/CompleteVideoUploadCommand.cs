using MedHub.Application.Abstractions.Messaging;
using MedHub.Application.Media.Contracts;

namespace MedHub.Application.Media.CompleteVideoUpload;

public sealed record CompleteVideoUploadCommand(
    Guid VideoId,
    string UploadId,
    IReadOnlyList<PartETagDto> PartETags
) : ICommand;