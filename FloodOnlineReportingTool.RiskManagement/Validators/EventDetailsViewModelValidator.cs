using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FluentValidation;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public partial class EventDetailsViewModelValidator : AbstractValidator<EventDetailsViewModel>
{
    public EventDetailsViewModelValidator()
    {
        RuleFor(o => o.SearchReference)
            .NotEmpty()
            .WithMessage("Search reference is required");

        RuleFor(o => o.EstimatedTotalInternallyFloodedString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for internally flooded");

        RuleFor(o => o.EstimatedTotalInternallyFlooded)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .OverridePropertyName(o => o.EstimatedTotalInternallyFloodedString)
            .When(o => !string.IsNullOrWhiteSpace(o.EstimatedTotalInternallyFloodedString));

        RuleFor(o => o.EstimatedTotalExternalOnlyString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for externally flooded");

        RuleFor(o => o.EstimatedTotalExternalOnly)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .OverridePropertyName(o => o.EstimatedTotalExternalOnlyString)
            .When(o => !string.IsNullOrWhiteSpace(o.EstimatedTotalExternalOnlyString));

        RuleFor(o => o.EstimatedDisruptedUtilitiesString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for disrupted utilities");

        RuleFor(o => o.EstimatedDisruptedUtilities)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .OverridePropertyName(o => o.EstimatedDisruptedUtilitiesString)
            .When(o => !string.IsNullOrWhiteSpace(o.EstimatedDisruptedUtilitiesString));

        RuleFor(o => o.EstimatedVulnerableString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for vulnerable persons");

        RuleFor(o => o.EstimatedVulnerable)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .OverridePropertyName(o => o.EstimatedVulnerableString)
            .When(o => !string.IsNullOrWhiteSpace(o.EstimatedVulnerableString));

        RuleFor(o => o.EstimatedUninhabitableString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for uninhabitable properties");

        RuleFor(o => o.EstimatedUninhabitable)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .OverridePropertyName(o => o.EstimatedUninhabitableString)
            .When(o => !string.IsNullOrWhiteSpace(o.EstimatedUninhabitableString));

        RuleFor(o => o.SearchTitle)
            .NotEmpty()
            .MinimumLength(25)
            .WithMessage("The title must be more than 25 characters")
            .MaximumLength(100)
            .WithMessage("The title must be less than 100 characters");

        RuleFor(o => o.StatusNote)
            .MaximumLength(500)
            .WithMessage("The status note must be less than 500 characters");
    }
}
