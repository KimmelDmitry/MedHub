namespace MedHub.Application.Media.Contracts;

public sealed record ChunkUploadUrlDto(
    int PartNumber,
    string UploadUrl
);