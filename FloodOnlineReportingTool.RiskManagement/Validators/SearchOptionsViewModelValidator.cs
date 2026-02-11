using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FluentValidation;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public partial class SearchOptionsViewModelValidator : AbstractValidator<SearchOptionsViewModel>
{
    public SearchOptionsViewModelValidator()
    {
        // When Report Reference is present we don't use other search criteria
        When(o => !string.IsNullOrWhiteSpace(o.ReportReference), () =>
        {
            RuleFor(o => o.ReportReference)
                .NotEmpty()
                .WithMessage("Enter a report reference");

            // All other fields must be null/empty when searching by report reference
            // Note: FromDate and ToDate are non-nullable so we don't validate them here
            RuleFor(o => o.ByPostcode).Empty().WithMessage("Postcode must not be set when searching by report reference");
            RuleFor(o => o.ByName).Empty().WithMessage("Name must not be set when searching by report reference");
            RuleFor(o => o.Easting).Null().WithMessage("Easting must not be set when searching by report reference");
            RuleFor(o => o.Northing).Null().WithMessage("Northing must not be set when searching by report reference");
            RuleFor(o => o.Buffer).Null().WithMessage("Buffer must not be set when searching by report reference");
            RuleFor(o => o.ByAreaWkt).Empty().WithMessage("Area must not be set when searching by report reference");
        });

        // All other options require options to be valid, and at least one must be populated
        When(o => string.IsNullOrWhiteSpace(o.ReportReference), () =>
        {
            RuleFor(o => o.ReportReference).Empty().WithMessage("Report reference must not be set for this search type");

            // At least one of the other fields must be populated
            RuleFor(o => o)
                .Must(o => IsDateSet(o.FromDate) || IsDateSet(o.ToDate) ||
                           !string.IsNullOrWhiteSpace(o.ByPostcode) ||
                           !string.IsNullOrWhiteSpace(o.ByName) ||
                           o.Easting.HasValue || o.Northing.HasValue ||
                           !string.IsNullOrWhiteSpace(o.ByAreaWkt))
                .WithMessage("Enter at least one search criterion")
                .WithName("SearchCriteria");

            // If Easting or Northing is set, all three (Easting, Northing, Buffer) must be set
            When(o => o.Easting.HasValue || o.Northing.HasValue, () =>
            {
                RuleFor(o => o.Easting)
                    .NotNull()
                    .WithMessage("Enter an easting value");

                RuleFor(o => o.Northing)
                    .NotNull()
                    .WithMessage("Enter a northing value");

                RuleFor(o => o.Buffer)
                    .NotNull()
                    .WithMessage("Select a buffer distance");
            });
        });
    }

    private static bool IsDateSet(GdsDate date)
    {
        // Check if the date has been set by the user (not just the default empty date)
        var defaultDate = new GdsDate();
        return date.Day != defaultDate.Day || date.Month != defaultDate.Month || date.Year != defaultDate.Year;
    }
}
