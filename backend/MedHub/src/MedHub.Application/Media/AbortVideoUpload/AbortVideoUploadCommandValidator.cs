using FluentValidation;

namespace MedHub.Application.Media.AbortVideoUpload;

public sealed class AbortVideoUploadCommandValidator
    : AbstractValidator<AbortVideoUploadCommand>
{
    public AbortVideoUploadCommandValidator()
    {
        RuleFor(x => x.VideoId)
            .NotEmpty();
    }
}