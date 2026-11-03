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
            RuleFor(o => o.ByPostcode).Empty().WithMessage("Postcode must not be set when searching by report reference");
            RuleFor(o => o.ByPersonsName).Empty().WithMessage("Name must not be set when searching by report reference");
            RuleFor(o => o.ByPolygon).Empty().WithMessage("Area must not be set when searching by report reference");
        });

        // All other options require options to be valid, and at least one must be populated
        When(o => string.IsNullOrWhiteSpace(o.ByReference), () =>
        {
            RuleFor(o => o.ByReference).Empty().WithMessage("Report reference must not be set for this search type");

            // At least date range, flood source and geometry must be populated
            RuleFor(o => o)
                .Must(o => IsDateSet(o.FromDate) && IsDateSet(o.ToDate) &&
                           o.ByFloodSource?.Count > 0 && 
                           o.ByPolygon != null)
                .WithMessage("Enter at least date range, flood source and map area")
                .WithName("SearchCriteria");

            // Date range validations
            RuleFor(o => o)
                .Must(o => IsDateSet(o.FromDate))
                .WithMessage("Enter a start date")
                .OverridePropertyName(o => o.FromDate);

            RuleFor(o => o)
                .Must(o => IsDateSet(o.ToDate))
                .WithMessage("Enter an end date")
                .OverridePropertyName(o => o.ToDate);

            // Flood source validations
            RuleFor(o => o.ByFloodSource)
                .Must(list => list == null || list.Count > 0)
                .WithMessage("Select at least one flood source")
                .OverridePropertyName(o => o.ByFloodSource);

            // Geometry validations
            RuleFor(o => o.ByPolygon)
                .Must(polygon => polygon == null || polygon.Count > 0)
                .WithMessage("Add a search area on the map")
                .OverridePropertyName(o => o.ByPolygon);

            // Date validations
            var now = DateTimeOffset.UtcNow;

            RuleFor(o => o.FromDate)
                .Cascade(CascadeMode.Stop)
                .DayMonthYearNotEmpty()
                .DayMustBeNumber()
                .MonthMustBeNumber()
                .YearMustBeNumber()
                .DayInclusiveBetween(1, 31)
                .MonthInclusiveBetween(1, 12)
                .YearInclusiveBetween(1900, now.Year + 1)
                .CorrectDaysInMonth()
                .IsRealDate()
                .WithMessage("Enter a valid date")
                .When(o => IsDateSet(o.ToDate));

            RuleFor(o => o.ToDate)
                .Cascade(CascadeMode.Stop)
                .DayMonthYearNotEmpty()
                .DayMustBeNumber()
                .MonthMustBeNumber()
                .YearMustBeNumber()
                .DayInclusiveBetween(1, 31)
                .MonthInclusiveBetween(1, 12)
                .YearInclusiveBetween(1900, now.Year + 1)
                .CorrectDaysInMonth()
                .IsRealDate()
                .WithMessage("Enter a valid date")
                .When(o => IsDateSet(o.ToDate));

            RuleFor(o => o.FromDate.DateUtc)
                .LessThanOrEqualTo(now)
                .WithMessage("Flooding start date must be today or in the past")
                .When(o => o.FromDate.DateUtc != null)
                .OverridePropertyName(o => o.FromDate);

            RuleFor(o => o.ToDate.DateUtc)
                .LessThanOrEqualTo(now)
                .WithMessage("Flooding end date must be today or in the past")
                .When(o => o.ToDate.DateUtc != null)
                .OverridePropertyName(o => o.ToDate);

            RuleFor(o => o.ToDate.DateUtc)
                .GreaterThanOrEqualTo(o => o.FromDate.DateUtc)
                .WithMessage("Flooding end date must be the same as or after the flooding start date")
                .When(o => o.FromDate.DateUtc != null && o.ToDate.DateUtc != null)
                .OverridePropertyName(o => o.ToDate);

            // UK Postcode validations
            RuleFor(o => o.ByPostcode)
                .Matches("^([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9]?[A-Za-z])))) [0-9][A-Za-z]{2})$")
                .WithMessage("Enter a valid UK postcode")
                .When(o => !string.IsNullOrWhiteSpace(o.ByPostcode))
                .OverridePropertyName(o => o.ByPostcode);

            // Reporter validations
            RuleFor(o => o.ByPersonsName)
                .MaximumLength(100)
                .WithMessage("Name must be 100 characters or fewer")
                .MinimumLength(2)
                .WithMessage("Name must be at least 2 characters")
                .When(o => !string.IsNullOrWhiteSpace(o.ByPersonsName))
                .OverridePropertyName(o => o.ByPersonsName);
        });
    }

    private static bool IsDateSet(GdsDate date)
    {
        // Check if the date has been set by the user (not just the default empty date)
        var defaultDate = new GdsDate();
        return date.Day != defaultDate.Day || date.Month != defaultDate.Month || date.Year != defaultDate.Year;
    }
}
