using System.Text.Json;

namespace Palaven.PdfIngest.FunctionApp;

public static class Utils
{
    public static bool TryToDeserializeMessage<T>(string message, out T documentAnalysisMessage)
    {
        var success = false;

        documentAnalysisMessage = default!;
        try
        {
            documentAnalysisMessage = JsonSerializer.Deserialize<T>(message)!;
            success = documentAnalysisMessage != null;
        }
        catch
        {
            throw;
        }

        return success;
    }
}
