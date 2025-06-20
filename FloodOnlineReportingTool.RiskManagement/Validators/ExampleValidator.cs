using FloodOnlineReportingTool.RiskManagement.Models;
using FluentValidation;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public partial class ExampleValidator : AbstractValidator<Example>
{
    public ExampleValidator()
    {
        RuleFor(o => o.FirstName)
            .NotEmpty()
            .WithMessage("Enter your first name");

        RuleFor(o => o.SomeNumber)
            .NotEmpty()
            .WithMessage("Enter some number");
    }
}
