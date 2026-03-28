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

        RuleFor(o => o.EstimatedResidentialFloodedString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for residential flooded");

        RuleFor(o => o.EstimatedResidentialFlooded)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedTotalInternallyFlooded)
            .WithMessage("You can't put a value greater than the total internally flooded properties");

        RuleFor(o => o.EstimatedCommercialFloodedString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for commercial flooded");

        RuleFor(o => o.EstimatedCommercialFlooded)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedTotalInternallyFlooded)
            .WithMessage("You can't put a value greater than the total internally flooded properties");

        RuleFor(o => o.EstimatedResidentialAffectedString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for residential affected");

        RuleFor(o => o.EstimatedResidentialAffected)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedTotalExternalOnly)
            .WithMessage("You can't put a value greater than the total externally affected properties");

        RuleFor(o => o.EstimatedCommercialAffectedString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for commercial affected");

        RuleFor(o => o.EstimatedCommercialAffected)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedTotalExternalOnly)
            .WithMessage("You can't put a value greater than the total externally affected properties");

        RuleFor(o => o.EstimatedResidentialOtherString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for residential other");

        RuleFor(o => o.EstimatedResidentialOther)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}");

        RuleFor(o => o.EstimatedCommercialOtherString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for commercial other");

        RuleFor(o => o.EstimatedCommercialOther)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}");

        RuleFor(o => o.EstimatedNoElectricString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for without electricity");

        RuleFor(o => o.EstimatedNoElectric)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedDisruptedUtilities)
            .WithMessage("You can't put a value greater than the total disrupted utilities");

        RuleFor(o => o.EstimatedNoGasString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for without gas");

        RuleFor(o => o.EstimatedNoGas)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedDisruptedUtilities)
            .WithMessage("You can't put a value greater than the total disrupted utilities");

        RuleFor(o => o.EstimatedNoFoulString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for without foul sewer");

        RuleFor(o => o.EstimatedNoFoul)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedDisruptedUtilities)
            .WithMessage("You can't put a value greater than the total disrupted utilities");

        RuleFor(o => o.EstimatedNoWaterSupplyString)
                .NotEmpty()
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for without water supply");

        RuleFor(o => o.EstimatedNoWaterSupply)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedDisruptedUtilities)
            .WithMessage("You can't put a value greater than the total disrupted utilities");
    }
}
