namespace MedHub.Application.Media.GetVideoHlsFile;

public sealed record VideoHlsFileResponse(
    Stream Content,
    string ContentType,
    long? ContentLength);
