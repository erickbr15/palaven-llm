using CsvHelper;
using System.Globalization;
using System.Text;
using CsvHelperConfiguration = CsvHelper.Configuration.CsvConfiguration;

namespace Palaven.Common;

public static class CsvUtility
{
    /// <summary>
    /// Generates a CSV file from a collection of data.
    /// </summary>
    /// <typeparam name="T">The type of the data to be written to the CSV.</typeparam>
    /// <param name="data">The collection of data to be converted to CSV format.</param>
    /// <param name="configuration">Optional CsvHelper configuration for customizing CSV output.</param>
    /// <returns>A byte array representing the generated CSV file.</returns>
    public static byte[] GenerateCsv<T>(IEnumerable<T> data, CsvConfiguration? configuration = null)
    {
        if (data == null || !data.Any())
        {
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));
        }
                    
        var csvHelperConfig = configuration == null ? 
            new CsvHelperConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true, // Include header row
                Encoding = Encoding.UTF8, // UTF-8 encoding
            } :
            new CsvHelperConfiguration(configuration.CultureInfo)
            {
                HasHeaderRecord = configuration.HasHeaderRecord, // Include header row
                Encoding = configuration.Encoding, // Use specified encoding
            };

        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, csvHelperConfig.Encoding);
        using var csvWriter = new CsvWriter(streamWriter, csvHelperConfig);

        csvWriter.WriteRecords(data); // Write data to CSV
        streamWriter.Flush(); // Ensure all data is written to the stream

        return memoryStream.ToArray(); // Return the CSV as a byte array
    }

    /// <summary>
    /// Reads a CSV file from a byte array and deserializes the content into a collection of data.
    /// </summary>
    /// <typeparam name="T">The type of the data to be deserialized.</typeparam>
    /// <param name="csvBytes">The byte array containing the CSV content.</param>
    /// <param name="configuration">Optional CsvHelper configuration for customizing CSV parsing.</param>
    /// <returns>An IEnumerable of the deserialized data.</returns>
    public static IEnumerable<T> ReadCsv<T>(byte[] csvBytes, CsvConfiguration? configuration = null)
    {
        if (csvBytes == null || csvBytes.Length == 0)
        {
            throw new ArgumentException("CSV bytes cannot be null or empty.", nameof(csvBytes));
        }

        var csvHelperConfig = configuration == null ?
            new CsvHelperConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true, // Include header row
                Encoding = Encoding.UTF8, // UTF-8 encoding
            } :
            new CsvHelperConfiguration(configuration.CultureInfo)
            {
                HasHeaderRecord = configuration.HasHeaderRecord, // Include header row
                Encoding = configuration.Encoding, // Use specified encoding
            };

        using var memoryStream = new MemoryStream(csvBytes);
        using var streamReader = new StreamReader(memoryStream, csvHelperConfig.Encoding);
        using var csvReader = new CsvReader(streamReader, csvHelperConfig);

        return csvReader.GetRecords<T>().ToList();
    }
}
