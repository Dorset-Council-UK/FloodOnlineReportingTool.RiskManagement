using FloodOnlineReportingTool.RiskManagement.Models.Report;
using FluentValidation;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public partial class ReportOverviewViewModelValidator : AbstractValidator<ReportOverviewViewModel>
{
    public ReportOverviewViewModelValidator()
    {
        //RuleFor(o => o.UprnText)
        //    .NotEmpty()
        //    .WithMessage("Enter a UPRN");

        //RuleFor(o => o.Uprn)
        //    .NotNull()
        //    .WithMessage("UPRN must be a number")
        //    .OverridePropertyName(o => o.UprnText)
        //    .When(o => !string.IsNullOrWhiteSpace(o.UprnText));
    }
}
