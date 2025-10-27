using FloodOnlineReportingTool.RiskManagement.Models;
using FluentValidation;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public partial class SearchFloodReportViewModelValidator : AbstractValidator<SearchFloodReportViewModel>
{
    public SearchFloodReportViewModelValidator()
    {
        RuleFor(o => o.SearchText)
            .NotEmpty()
            .WithMessage("Enter something to search for");

        RuleFor(o => o.UprnText)
            .NotEmpty()
            .WithMessage("Enter a Unique Property Reference Number to search for");

        RuleFor(o => o.Uprn)
            .NotNull()
            .WithMessage("Unique Property Reference must be a number")
            .When(o => !string.IsNullOrWhiteSpace(o.UprnText));
    }
}
