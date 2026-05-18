namespace MedHub.Application.Media.Contracts;

public sealed record PartETagDto(
    int PartNumber,
    string ETag
);
