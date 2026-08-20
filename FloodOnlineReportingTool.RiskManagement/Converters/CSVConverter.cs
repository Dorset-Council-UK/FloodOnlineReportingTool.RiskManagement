using CsvHelper;
using Microsoft.JSInterop;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Converters;

/// <summary>
/// A class to manage CSV file manipulation. 
/// </summary>
public static class CSVConverter
{
    /// <summary>
    /// Create a CSV file from a given object and return it as a DotNetStreamReference for download.
    /// </summary>
    /// <typeparam name="T">The (single) type of object to write to a CSV file.</typeparam>
    /// <param name="CSVDataCreator">The CSVDataCreator instance used to convert the object into CSV data.</param>
    /// <param name="objectToWriteToCSV">The object to write to the CSV file.</param>
    /// <returns>A DotNetStreamReference representing the created CSV file.</returns>
    public static DotNetStreamReference CreateCsv<T>(CSVDataCreator<T> CSVDataCreator, T? objectToWriteToCSV)
        where T : class
    {
        // Write csv data to Memory Stream
        MemoryStream ms = new();

        using var writer = new StreamWriter(ms, leaveOpen: true);        
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        CSVDataCreator.WriteToMemoryStream(objectToWriteToCSV, csv);
        writer.Flush();
        
        // Set pointer to start of memory stream for export
        ms.Position = 0;

        // return file created from memory stream
        return new DotNetStreamReference(stream: ms);
    }
}
