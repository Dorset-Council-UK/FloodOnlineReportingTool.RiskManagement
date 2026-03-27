using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FluentValidation;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public partial class EventEstimatesViewModelValidator : AbstractValidator<EventEstimatesViewModel>
{
    public EventEstimatesViewModelValidator()
    {
        RuleFor(o => o.SearchReference)
            .NotEmpty()
            .WithMessage("Search reference is required");
        
        RuleFor(o => o.EstimatedCommercialFlooded)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedTotalInternallyFlooded)
            .WithMessage("You can't put a value greater than the total internally flooded properties");

        RuleFor(o => o.EstimatedResidentialFlooded)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedTotalInternallyFlooded)
            .WithMessage("You can't put a value greater than the total internally flooded properties");


        RuleFor(o => o.EstimatedResidentialAffected)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedTotalExternalOnly)
            .WithMessage("You can't put a value greater than the total externally affected properties");

        RuleFor(o => o.EstimatedCommercialAffected)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedTotalExternalOnly)
            .WithMessage("You can't put a value greater than the total externally affected properties");

        RuleFor(o => o.EstimatedResidentialOther)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}");

        RuleFor(o => o.EstimatedCommercialOther)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}");

        RuleFor(o => o.EstimatedNoElectric)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedDisruptedUtilities)
            .WithMessage("You can't put a value greater than the total disrupted utilities");

        RuleFor(o => o.EstimatedNoGas)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedDisruptedUtilities)
            .WithMessage("You can't put a value greater than the total disrupted utilities");

        RuleFor(o => o.EstimatedNoFoul)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedDisruptedUtilities)
            .WithMessage("You can't put a value greater than the total disrupted utilities");

        RuleFor(o => o.EstimatedNoWaterSupply)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedDisruptedUtilities)
            .WithMessage("You can't put a value greater than the total disrupted utilities");
    }
}
