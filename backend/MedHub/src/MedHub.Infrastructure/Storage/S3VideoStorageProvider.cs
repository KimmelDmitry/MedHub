using Amazon.S3;
using Amazon.S3.Model;
using MedHub.Application.Abstractions.Storage;
using MedHub.Application.Media.Contracts;
using Microsoft.Extensions.Options;

namespace MedHub.Infrastructure.Storage;

public sealed class S3VideoStorageProvider : IVideoStorageProvider
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3VideoStorageProvider(
        IAmazonS3 s3Client,
        IOptions<FileStorageOptions> options)
    {
        _s3Client = s3Client;
        _bucketName = options.Value.BucketName;
    }

    public async Task<MultipartUploadInitDto> StartMultipartUploadAsync(
        string objectKey,
        string contentType,
        long sizeBytes,
        CancellationToken ct = default)
    {
        var request = new InitiateMultipartUploadRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            ContentType = contentType
        };

        var response = await _s3Client.InitiateMultipartUploadAsync(request, ct);

        return new MultipartUploadInitDto(response.UploadId);
    }

    public async Task<IReadOnlyList<ChunkUploadUrlDto>> GetUploadUrlsAsync(
        string objectKey,
        string uploadId,
        int partsCount,
        CancellationToken ct = default)
    {
        var urls = new List<ChunkUploadUrlDto>(partsCount);

        for (int partNumber = 1; partNumber <= partsCount; partNumber++)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            request.Parameters["partNumber"] = partNumber.ToString();
            request.Parameters["uploadId"] = uploadId;

            var url = _s3Client.GetPreSignedURL(request);

            urls.Add(new ChunkUploadUrlDto(partNumber, url));
        }

        return urls;
    }

    public async Task CompleteMultipartUploadAsync(
        string objectKey,
        string uploadId,
        IReadOnlyList<PartETagDto> parts,
        CancellationToken ct = default)
    {
        var request = new CompleteMultipartUploadRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            UploadId = uploadId
        };

        request.PartETags.AddRange(parts.Select((p => new PartETag(p.PartNumber, p.ETag))));

        await _s3Client.CompleteMultipartUploadAsync(request, ct);
    }

    public async Task AbortMultipartUploadAsync(
        string objectKey,
        string uploadId,
        CancellationToken ct = default)
    {
        var request = new AbortMultipartUploadRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            UploadId = uploadId
        };

        await _s3Client.AbortMultipartUploadAsync(request, ct);
    }

    public async Task DownloadFileAsync(
        string objectKey,
        string destinationPath,
        CancellationToken ct = default)
    {
        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey
        };

        using var response = await _s3Client.GetObjectAsync(request, ct);
        await using var responseStream = response.ResponseStream;
        await using var fileStream = File.Create(destinationPath);

        await responseStream.CopyToAsync(fileStream, ct);
    }

    public async Task<Stream> OpenReadAsync(
        string objectKey,
        CancellationToken ct = default)
    {
        var response = await _s3Client.GetObjectAsync(_bucketName, objectKey, ct);
        return response.ResponseStream;
    }

    public async Task UploadStreamAsync(
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false
        };

        await _s3Client.PutObjectAsync(request, ct);
    }

    public Task<string> GetPlaybackUrlAsync(
        string objectKey,
        CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddHours(1),
            Verb = HttpVerb.GET
        };

        var url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(url);
    }
}