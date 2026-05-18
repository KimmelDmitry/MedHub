namespace MedHub.Application.Media.StartVideoUpload;

public sealed record StartVideoUploadResult(
    Guid VideoId,
    string UploadId,
    int ChunkSize,
    IReadOnlyList<ChunkUploadUrl> ChunkUploadUrls
);