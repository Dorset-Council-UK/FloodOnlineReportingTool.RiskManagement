using System.Globalization;

namespace System;

internal static class DateTimeOffsetExtensions
{
    extension(DateTimeOffset dateTimeOffset)
    {
        internal string GdsReadable(bool showYear = true)
        {
            var format = showYear ? "d MMMM yyyy" : "d MMMM";
            return dateTimeOffset.ToString(format, CultureInfo.CurrentCulture);
        }
    }

    extension(DateTimeOffset? dateTimeOffset)
    {
        internal string GdsReadable(bool showYear = true)
        {
            if (dateTimeOffset is null)
            {
                return string.Empty;
            }

            return dateTimeOffset.Value.GdsReadable(showYear);
        }
    }
}
