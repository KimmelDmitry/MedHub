using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Media.AbortVideoUpload;

public sealed record AbortVideoUploadCommand(
    Guid VideoId
) : ICommand;