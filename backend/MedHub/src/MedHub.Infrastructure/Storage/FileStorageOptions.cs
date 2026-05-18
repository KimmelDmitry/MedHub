namespace MedHub.Infrastructure.Storage;

public sealed class FileStorageOptions
{
    public string Endpoint { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;
    
    // Добавь это поле, если его нет
    public bool UseHttps { get; init; } = false; 
}