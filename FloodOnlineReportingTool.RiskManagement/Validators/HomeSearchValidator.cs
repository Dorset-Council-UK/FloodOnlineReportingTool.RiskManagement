using FloodOnlineReportingTool.RiskManagement.Models.Searches;
using FluentValidation;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public partial class HomeSearchValidator : AbstractValidator<HomeSearch>
{
    public HomeSearchValidator()
    {
        RuleFor(o => o.SearchText)
            .NotEmpty()
            .WithMessage("Enter something to search for");
    }
}
