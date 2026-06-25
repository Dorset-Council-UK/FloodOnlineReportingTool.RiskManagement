namespace GdsBlazorComponents;

internal static class GdsDateExtensions
{
    extension(GdsDate gdsDate)
    {
        /// <summary>
        /// Copy the values from a <see cref="DateTimeOffset"/> into the target <see cref="GdsDate"/>.
        /// </summary>
        /// <remarks>This copies the values, never creates a new GdsDate.</remarks>
        internal void CopyFrom(DateTimeOffset? dateTimeOffset)
        {
            if (dateTimeOffset is null)
            {
                gdsDate.DayText = null;
                gdsDate.MonthText = null;
                gdsDate.YearText = null;
                return;
            }

            // When GdsDate is improved we can use DateTimeOffset day, month, year properties directly instead of creating a new GdsDate instance.
            var source = new GdsDate(dateTimeOffset.Value);
            gdsDate.DayText = source.DayText;
            gdsDate.MonthText = source.MonthText;
            gdsDate.YearText = source.YearText;
        }

        internal void CopyFrom(GdsDate source)
        {
            // When GdsDate is improved we can set either day, month, year, or the text properties
            gdsDate.DayText = source.DayText;
            gdsDate.MonthText = source.MonthText;
            gdsDate.YearText = source.YearText;
        }
    }
}
