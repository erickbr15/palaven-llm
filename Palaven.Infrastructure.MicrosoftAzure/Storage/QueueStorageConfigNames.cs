using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Infrastructure.MicrosoftAzure.Storage;

public static class QueueStorageConfigNames
{
    public const string DocumentAnalysisQueue = "DocumentAnalysis";
    public const string ExtractDocumentPagesQueue = "ExtractDocumentPages";
    public const string ExtractArticleParagraphsQueue = "ExtractArticleParagraphs";
    public const string CurateArticlesQueue = "CurateArticles";
    public const string GenerateInstructionsQueue = "GenerateInstructions";
    public const string IndexInstructionsQueue = "IndexInstructions";
    public const string CreateInstructionsDatasetQueue = "CreateInstructionsDataset";

    public static string GetQueueConfigNameByMessageType(Type messageType)
    {
        return messageType switch
        {
            Type t when t == typeof(DocumentAnalysisMessage) => DocumentAnalysisQueue,
            Type t when t == typeof(ExtractDocumentPagesMessage) => ExtractDocumentPagesQueue,
            Type t when t == typeof(ExtractArticleParagraphsMessage) => ExtractArticleParagraphsQueue,
            Type t when t == typeof(CurateArticlesMessage) => CurateArticlesQueue,
            Type t when t == typeof(GenerateInstructionsMessage) => GenerateInstructionsQueue,
            Type t when t == typeof(IndexInstructionsMessage) => IndexInstructionsQueue,
            Type t when t == typeof(CreateInstructionDatasetMessage) => CreateInstructionsDatasetQueue,
            _ => throw new InvalidOperationException($"Unknown message type: {messageType.Name}")
        };
    }
}
