using FloodOnlineReportingTool.RiskManagement.Models;
using FluentValidation;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public partial class FloodEventViewModelValidator : AbstractValidator<FloodEventViewModel>
{
    public FloodEventViewModelValidator()
    {
        RuleFor(o => o.Something)
            .NotEmpty()
            .WithMessage("Enter something");
    }
}
