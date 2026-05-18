using FluentValidation;

namespace MedHub.Application.Media.CompleteVideoUpload;

public sealed class CompleteVideoUploadCommandValidator
    : AbstractValidator<CompleteVideoUploadCommand>
{
    public CompleteVideoUploadCommandValidator()
    {
        RuleFor(x => x.VideoId)
            .NotEmpty();

        RuleFor(x => x.UploadId)
            .NotEmpty();

        RuleFor(x => x.PartETags)
            .NotEmpty();

        RuleForEach(x => x.PartETags).ChildRules(part =>
        {
            part.RuleFor(x => x.PartNumber)
                .GreaterThan(0);

            part.RuleFor(x => x.ETag)
                .NotEmpty();
        });
    }
}