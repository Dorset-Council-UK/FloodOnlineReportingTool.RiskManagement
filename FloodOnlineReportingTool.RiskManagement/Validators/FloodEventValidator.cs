using FloodOnlineReportingTool.RiskManagement.Models;
using FluentValidation;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public partial class FloodEventValidator : AbstractValidator<FloodEvent>
{
    public FloodEventValidator()
    {
        RuleFor(o => o.FloodStartDate)
            .NotEmpty()
            .WithMessage("Enter a start date for the flood");

        //TODO add more validations
    }
}
