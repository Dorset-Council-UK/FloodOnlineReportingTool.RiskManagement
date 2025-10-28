using FloodOnlineReportingTool.RiskManagement.Models;
using FluentValidation;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public partial class SearchFloodReportViewModelValidator : AbstractValidator<SearchFloodReportViewModel>
{
    public SearchFloodReportViewModelValidator()
    {
        RuleFor(o => o.UprnText)
            .NotEmpty()
            .WithMessage("Enter a UPRN");

        RuleFor(o => o.Uprn)
            .NotNull()
            .WithMessage("UPRN must be a number")
            .When(o => !string.IsNullOrWhiteSpace(o.UprnText));
    }
}
