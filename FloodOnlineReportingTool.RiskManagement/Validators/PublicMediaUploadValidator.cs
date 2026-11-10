using FloodOnlineReportingTool.Database.Models.Media;
using FloodOnlineReportingTool.RiskManagement.Models.Media;
using FluentValidation;
using FluentValidation.Internal;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public class PublicMediaUploadValidator : AbstractValidator<PublicMediaUpload>
{
    public PublicMediaUploadValidator()
    {
        RuleFor(m => m.Name)
            .Must(name => PublicMediaSubmissionInput.NormalizeContact(name)?.Length is not > PublicMediaSubmissionInput.MaxNameLength)
            .WithMessage("Name must be 200 characters or fewer");

        RuleFor(m => m.EmailAddress)
            .Must(email => PublicMediaSubmissionInput.NormalizeContact(email)?.Length is not > PublicMediaSubmissionInput.MaxEmailLength)
            .WithMessage("Email address must be 254 characters or fewer")
            .Must(PublicMediaSubmissionInput.IsValidEmail)
            .WithMessage("Enter a valid email address");

        RuleFor(m => m.SharingAgreementAccepted)
            .Equal(true)
            .WithMessage("You must agree that your media can be shared before submitting");

        RuleFor(m => m.UploadedFiles)
            .Must(files => files.Count is > 0 and <= 50)
            .WithMessage("Upload between 1 and 50 files before submitting")
            // The file input notifies field changes before selected files have been uploaded.
            .When((_, context) => context.Selector is not MemberNameValidatorSelector);
    }
}
