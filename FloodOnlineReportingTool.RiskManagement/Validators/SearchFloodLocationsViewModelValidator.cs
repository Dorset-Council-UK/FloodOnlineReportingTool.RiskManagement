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
                .WithMessage("Select a buffer distance")
                .GreaterThan(0)
                .WithMessage("Buffer distance must be greater than 0");
        });

        // pagnation validation
        RuleFor(o => o.PageNumber)
            .NotNull()
            .WithMessage("Page number must be greater than 0")
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");
    }
}
