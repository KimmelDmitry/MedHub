namespace MedHub.Application.Media.StartVideoUpload;

public sealed record ChunkUploadUrl(
    int PartNumber,
    string UploadUrl
);