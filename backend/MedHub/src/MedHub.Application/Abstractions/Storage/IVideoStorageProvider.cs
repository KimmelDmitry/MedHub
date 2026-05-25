using MedHub.Application.Media.Contracts;

namespace MedHub.Application.Abstractions.Storage;

public interface IVideoStorageProvider
{
    Task<MultipartUploadInitDto> StartMultipartUploadAsync(
        string objectKey,
        string contentType,
        long sizeBytes,
        CancellationToken ct = default);

    Task<IReadOnlyList<ChunkUploadUrlDto>> GetUploadUrlsAsync(
        string objectKey,
        string uploadId,
        int partsCount,
        CancellationToken ct = default);

    Task CompleteMultipartUploadAsync(
        string objectKey,
        string uploadId,
        IReadOnlyList<PartETagDto> parts,
        CancellationToken ct = default);

    Task AbortMultipartUploadAsync(
        string objectKey,
        string uploadId,
        CancellationToken ct = default);

    Task DownloadFileAsync(
        string objectKey,
        string destinationPath,
        CancellationToken ct = default);

    Task UploadStreamAsync(
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken ct = default);

    Task<StorageObjectStream> OpenReadAsync(
        string objectKey,
        CancellationToken ct = default);

    Task<string> GetPlaybackUrlAsync(
        string objectKey,
        CancellationToken ct = default);
}

public sealed record MultipartUploadInitDto(
    string UploadId
);

public sealed record StorageObjectStream(
    Stream Content,
    string? ContentType,
    long? ContentLength);
