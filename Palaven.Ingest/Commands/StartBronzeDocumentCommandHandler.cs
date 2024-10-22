using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Palaven.Ingest.Resources;
using Palaven.Model.Data.Documents;
using Palaven.Model.Ingest;
using System.Net;

namespace Palaven.Ingest.Commands;

public class StartBronzeDocumentCommandHandler : ICommandHandler<StartBronzeDocumentCommand, string>
{    
    private readonly DocumentAnalysisClient _documentAnalysisClient;
    private readonly IDocumentRepository<EtlTaskDocument> _taskDocumentRepository;

    public StartBronzeDocumentCommandHandler(DocumentAnalysisClient documentAnalysisClient, IDocumentRepository<EtlTaskDocument> taskDocumentRepository)
    {
        _documentAnalysisClient = documentAnalysisClient ?? throw new ArgumentNullException(nameof(documentAnalysisClient));
        _taskDocumentRepository = taskDocumentRepository ?? throw new ArgumentNullException(nameof(taskDocumentRepository));        
    }

    public async Task<IResult<string>> ExecuteAsync(StartBronzeDocumentCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return await Task.FromResult(Result<string>.Fail(new List<ValidationError>(), new List<Exception> { new ArgumentNullException(nameof(command)) }));
        }

        var etlTask = await FetchLatestEtlTaskVersionAsync(command.OperationId, cancellationToken);
        if (etlTask == null)
        {
            return await Task.FromResult(Result<string>.Fail(new List<ValidationError>(), new List<Exception> { new InvalidOperationException($"Unable to find the ETL task document with id {command.OperationId}") }));
        }

        var analyzeDocumentOptions = CreateAnalyzeDocumentOptions(command);
        var analysisOperation = await _documentAnalysisClient.AnalyzeDocumentAsync(WaitUntil.Started, "prebuilt-layout", 
            command.DocumentContent, 
            analyzeDocumentOptions,
            cancellationToken: cancellationToken);
        
        etlTask.Details.Add($"{DateTime.UtcNow.ToString()}.{Etl.BronzeStageStartDocumentAnalysis}. OperationId: {analysisOperation.Id}");
        
        var dbResponse = await _taskDocumentRepository.UpsertAsync(etlTask, new PartitionKey(etlTask.UserId.ToString()), itemRequestOptions: null, cancellationToken);
        if(dbResponse.StatusCode != HttpStatusCode.OK)
        {
            return await Task.FromResult(Result<string>.Fail(new List<ValidationError>(), 
                new List<Exception> { new InvalidOperationException($"Unable to update the ETL task document. Status code: {dbResponse.StatusCode}") }));
        }

        var bronzeStageMessage = CreateBronzeStageMessage(command, analysisOperation.Id);

        return Result<string>.Success(bronzeStageMessage);
    }

    private async Task<EtlTaskDocument?> FetchLatestEtlTaskVersionAsync(Guid operationId, CancellationToken cancellationToken)
    {
        var query = new QueryDefinition($"SELECT * FROM c WHERE c.id = '{operationId}'");

        var queryResults = await _taskDocumentRepository.GetAsync(
            query,
            continuationToken: null,
            queryRequestOptions: null,
            cancellationToken);

        return queryResults.SingleOrDefault();
    }

    private AnalyzeDocumentOptions? CreateAnalyzeDocumentOptions(StartBronzeDocumentCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.DocumentLocale) && !command.DocumentPages.Any())
        {
            return null;
        }

        var options = new AnalyzeDocumentOptions();
        if (!string.IsNullOrWhiteSpace(command.DocumentLocale))
        {
            options.Locale = command.DocumentLocale;
        }

        command.DocumentPages.ToList().ForEach(p => options.Pages.Add(p));

        return options;
    }
    
    private string CreateBronzeStageMessage(StartBronzeDocumentCommand command, string documentAnalysisOperationId)
    {
        var message = new BronzeStageMessage
        {
            OperationId = command.OperationId.ToString(),
            DocumentAnalysisOperationId = documentAnalysisOperationId
        };

        return JsonConvert.SerializeObject(message);
    }
}
