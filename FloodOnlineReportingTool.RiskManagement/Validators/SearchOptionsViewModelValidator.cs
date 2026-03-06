using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FluentValidation;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public partial class SearchOptionsViewModelValidator : AbstractValidator<SearchOptionsViewModel>
{
    public SearchOptionsViewModelValidator()
    {
        // When Report Reference is present we don't use other search criteria
        When(o => !string.IsNullOrWhiteSpace(o.ByReference), () =>
        {
            RuleFor(o => o.ByReference)
                .NotEmpty()
                .WithMessage("Enter a report reference");

            // All other fields must be null/empty when searching by report reference
            // Note: FromDate and ToDate are non-nullable so we don't validate them here
            RuleFor(o => o.ByPlaceName).Empty().WithMessage("Postcode must not be set when searching by report reference");
            RuleFor(o => o.ByPersonsName).Empty().WithMessage("Name must not be set when searching by report reference");
            RuleFor(o => o.Buffer).Null().WithMessage("Buffer must not be set when searching by report reference");
            RuleFor(o => o.ByPolygon).Empty().WithMessage("Area must not be set when searching by report reference");
        });

        // All other options require options to be valid, and at least one must be populated
        When(o => string.IsNullOrWhiteSpace(o.ByReference), () =>
        {
            RuleFor(o => o.ByReference).Empty().WithMessage("Report reference must not be set for this search type");

            // At least one of the other fields must be populated
            RuleFor(o => o)
                .Must(o => IsDateSet(o.FromDate) || IsDateSet(o.ToDate) ||
                           !string.IsNullOrWhiteSpace(o.ByPlaceName) ||
                           !string.IsNullOrWhiteSpace(o.ByPersonsName) ||
                           o.ByPolygon != null)
                .WithMessage("Enter at least one search criterion")
                .WithName("SearchCriteria");
        });
    }

    private static bool IsDateSet(GdsDate date)
    {
        // Check if the date has been set by the user (not just the default empty date)
        var defaultDate = new GdsDate();
        return date.Day != defaultDate.Day || date.Month != defaultDate.Month || date.Year != defaultDate.Year;
    }
}
