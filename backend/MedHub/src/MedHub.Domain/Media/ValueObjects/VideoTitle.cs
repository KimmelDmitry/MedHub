using MedHub.Domain.Abstractions;

namespace MedHub.Domain.Media.ValueObjects;

public sealed record VideoTitle
{
    public string Value { get; }
    
    private const int MIN_LENGHT = 3;
    private const int MAX_LENGHT = 300;

    private VideoTitle(string value) => Value = value;

    public static readonly Error InvalidLength = new(
        "VideoTitle.InvalidLength", 
        $"Название видео должно быть от {MIN_LENGHT} до {MAX_LENGHT} символов");

    public static Result<VideoTitle> Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<VideoTitle>(Error.NullValue); // Или специфичную ошибку TitleRequired

        if (title.Length < MIN_LENGHT || title.Length > MAX_LENGHT)
            return Result.Failure<VideoTitle>(InvalidLength);

        return new VideoTitle(title.Trim());
    }
}