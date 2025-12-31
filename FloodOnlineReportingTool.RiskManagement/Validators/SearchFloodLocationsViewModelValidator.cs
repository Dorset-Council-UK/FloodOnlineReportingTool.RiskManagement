using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FluentValidation;

namespace FloodOnlineReportingTool.RiskManagement.Validators;

public partial class SearchFloodLocationsViewModelValidator : AbstractValidator<SearchFloodLocationsViewModel>
{
    public SearchFloodLocationsViewModelValidator()
    {
        RuleFor(o => o.SearchType)
            .IsInEnum()
            .WithMessage("Select a search type");

        When(o => o.SearchType == FloodLocationSearchType.ByLocation, () =>
        {
            RuleFor(o => o.Easting)
                .NotNull()
                .WithMessage("Easting must be a number")
                .OverridePropertyName(o => o.EastingText)
                .When(o => !string.IsNullOrWhiteSpace(o.EastingText));

            RuleFor(o => o.Northing)
                .NotNull()
                .WithMessage("Northing must be a number")
                .OverridePropertyName(o => o.NorthingText)
                .When(o => !string.IsNullOrWhiteSpace(o.NorthingText));

            RuleFor(o => o.Buffer)
                .NotNull()
                .WithMessage("Select a buffer distance");
        });

        // pagnation validation
        RuleFor(o => o.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0")
            .OverridePropertyName(o => o.PageNumberText)
            .When(o => !string.IsNullOrWhiteSpace(o.PageNumberText));

        RuleFor(o => o.PageSize)
            .GreaterThan(0)
            .WithMessage("Select a page size")
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");
    }
}
