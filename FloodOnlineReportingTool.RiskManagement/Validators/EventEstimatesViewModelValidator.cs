using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FluentValidation;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public partial class EventEstimatesViewModelValidator : AbstractValidator<EventEstimatesViewModel>
{
#pragma warning disable MA0051 // Method is too long
    public EventEstimatesViewModelValidator()
#pragma warning restore MA0051 // Method is too long
    {
        RuleFor(o => o.SearchReference)
            .NotEmpty()
            .WithMessage("Search reference is required");

        RuleFor(o => o.EstimatedResidentialFloodedString)
                .NotEmpty()
                .WithMessage("Enter a number for residential flooded")
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for residential flooded");

        RuleFor(o => o.EstimatedResidentialFlooded)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedTotalInternallyFlooded)
            .WithMessage("You can't put a value greater than the total internally flooded properties");

        RuleFor(o => o.EstimatedCommercialFloodedString)
                .NotEmpty()
                .WithMessage("Enter a number for commercial flooded")
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for commercial flooded");

        RuleFor(o => o.EstimatedCommercialFlooded)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedTotalInternallyFlooded)
            .WithMessage("You can't put a value greater than the total internally flooded properties");

        RuleFor(o => o.EstimatedResidentialAffectedString)
                .NotEmpty()
                .WithMessage("Enter a number for residential affected")
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for residential affected");

        RuleFor(o => o.EstimatedResidentialAffected)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedTotalExternalOnly)
            .WithMessage("You can't put a value greater than the total externally affected properties");

        RuleFor(o => o.EstimatedCommercialAffectedString)
                .NotEmpty()
                .WithMessage("Enter a number for commercial affected")
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for commercial affected");

        RuleFor(o => o.EstimatedCommercialAffected)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedTotalExternalOnly)
            .WithMessage("You can't put a value greater than the total externally affected properties");

        RuleFor(o => o.EstimatedResidentialOtherString)
                .NotEmpty()
                .WithMessage("Enter a number for residential other")
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for residential other");

        RuleFor(o => o.EstimatedResidentialOther)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}");

        RuleFor(o => o.EstimatedCommercialOtherString)
                .NotEmpty()
                .WithMessage("Enter a number for commercial other")
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for commercial other");

        RuleFor(o => o.EstimatedCommercialOther)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}");

        RuleFor(o => o.EstimatedNoElectricString)
                .NotEmpty()
                .WithMessage("Enter a number for without electricity")
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for without electricity");

        RuleFor(o => o.EstimatedNoElectric)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedDisruptedUtilities)
            .WithMessage("You can't put a value greater than the total disrupted utilities");

        RuleFor(o => o.EstimatedNoGasString)
                .NotEmpty()
                .WithMessage("Enter a number for without gas")
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for without gas");

        RuleFor(o => o.EstimatedNoGas)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedDisruptedUtilities)
            .WithMessage("You can't put a value greater than the total disrupted utilities");

        RuleFor(o => o.EstimatedNoFoulString)
                .NotEmpty()
                .WithMessage("Enter a number for without foul sewer")
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for without foul sewer");

        RuleFor(o => o.EstimatedNoFoul)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedDisruptedUtilities)
            .WithMessage("You can't put a value greater than the total disrupted utilities");

        RuleFor(o => o.EstimatedNoWaterSupplyString)
                .NotEmpty()
                .WithMessage("Enter a number for without water supply")
                .Matches("^[0-9]*$")
                .WithMessage("Enter a number for without water supply");

        RuleFor(o => o.EstimatedNoWaterSupply)
            .InclusiveBetween(0, 5000)
            .WithMessage("The number must be between {From} and {To}")
            .LessThanOrEqualTo(o => o.EstimatedDisruptedUtilities)
            .WithMessage("You can't put a value greater than the total disrupted utilities");
    }
}
