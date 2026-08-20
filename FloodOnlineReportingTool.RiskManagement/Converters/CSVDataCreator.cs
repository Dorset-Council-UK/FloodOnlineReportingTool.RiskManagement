using CsvHelper;
using System.Globalization;
using System.Numerics;

namespace FloodOnlineReportingTool.RiskManagement.Converters;

/// <summary>
/// Objects inheriting from this class can be used to create CSV files from a given object of type T.
/// </summary>
/// <typeparam name="T">The type of object to create a CSV file from.</typeparam>
/// <param name="Headers">The collection of properties to use as header names for the CSV file.</param>
public abstract record CSVDataCreator<T>(IReadOnlyCollection<string> Headers) where T : class
{
    /// <summary>
    /// Writes the provided object to the provided CsvWriter instance, including a header row of properties.
    /// </summary>
    /// <param name="objectToWriteToCSV">The object to write to the CSV file.</param>
    /// <param name="csvWriter">The CsvWriter instance to write the object to.</param>
    public void WriteToMemoryStream(T? objectToWriteToCSV, CsvWriter csvWriter)
    {
        if (objectToWriteToCSV is null)
            return;

        WriteHeaderToMemoryStream(csvWriter);
        WriteObjectToMemoryStream(objectToWriteToCSV, csvWriter);
        csvWriter.NextRecord();
    }

    /// <summary>
    /// Writes a header row of properties to the provided CsvWriter instance.
    /// </summary>
    /// <param name="csvWriter">The CsvWriter instance to write the header row to.</param>
    protected void WriteHeaderToMemoryStream(CsvWriter csvWriter)
    {
        foreach (var header in Headers)
            csvWriter.WriteField(header);

        csvWriter.NextRecord();
    }

    /// <summary>
    /// Writes the provided object to the CsvWriter instance.
    /// </summary>
    /// <param name="objectToWriteToCSV">The object to write to the CSV file.</param>
    /// <param name="csvWriter">The CsvWriter instance to write the object to.</param>
    protected abstract void WriteObjectToMemoryStream(T objectToWriteToCSV, CsvWriter csvWriter);

    /// <summary>
    /// Converts a list of strings into a single comma-separated string.
    /// </summary>
    /// <param name="values">The list of strings to convert.</param>
    /// <returns>A single string with the values joined by commas.</returns>
    protected string StringListToSingleValue(IReadOnlyCollection<string> values)
        => string.Join(", ", values);

    /// <summary>
    /// Converts a nullable integer-type number to a string representation.
    /// </summary>
    /// <typeparam name="TNumber">The integer type of the number.</typeparam>
    /// <param name="value">The nullable number to convert.</param>
    /// <returns>A string representation of the number using the invariant culture or an empty string if the value is null.</returns>
    protected string NullableNumberToString<TNumber>(TNumber? value)
        where TNumber : struct, IBinaryInteger<TNumber>
        => value?.ToString("0", CultureInfo.InvariantCulture) ?? string.Empty;
}
