using MedHub.Application.Media.Contracts;

namespace MedHub.Api.Controllers.Media;

public sealed record CompleteVideoUploadRequest(
    string UploadId,
    IReadOnlyList<PartETagDto> PartETags
);