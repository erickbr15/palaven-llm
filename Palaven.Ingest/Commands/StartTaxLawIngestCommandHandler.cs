using Azure.Storage.Blobs.Models;
using Liara.Azure.BlobStorage;
using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Data.Documents;
using Palaven.Model.Ingest;
using System.Net;

namespace Palaven.Ingest.Commands;

public class StartTaxLawIngestCommandHandler : ICommandHandler<StartTaxLawIngestCommand, EtlTaskDocument>
{
    private readonly IBlobStorageService _storageService;
    private readonly IDocumentRepository<StartTaxLawIngestTaskDocument> _repository;

    public StartTaxLawIngestCommandHandler(IBlobStorageService blobStorageService, IDocumentRepository<StartTaxLawIngestTaskDocument> repository)
    {
        _storageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IResult<EtlTaskDocument>> ExecuteAsync(StartTaxLawIngestCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return await Task.FromResult(Result<EtlTaskDocument>.Fail(new List<ValidationError>(), new List<Exception> { new ArgumentNullException(nameof(command)) }));
        }

        var documentId = Guid.NewGuid();
        var traceId = Guid.NewGuid();
        var fileName = $"{documentId}{command.FileExtension}";

        var lawToIngestDocument = new StartTaxLawIngestTaskDocument
        {
            Id = documentId.ToString(),
            TraceId = traceId,
            UserId = command.UserId,
            FileName = fileName,
            UntrustedFileName = command.UntrustedFileName,
            AcronymLaw = command.AcronymLaw,
            NameLaw = command.NameLaw,
            YearLaw = command.YearLaw,
            DocumentSchema = nameof(StartTaxLawIngestTaskDocument),
            IsTaskCompleted = false
        };        
        
        var result = await _repository.CreateAsync(lawToIngestDocument, new PartitionKey(lawToIngestDocument.YearLaw), itemRequestOptions: null, cancellationToken);

        if (result.StatusCode != HttpStatusCode.Created)
        {
            throw new InvalidOperationException($"Unable to create {nameof(StartTaxLawIngestTaskDocument)} document. Status code: {result.StatusCode}");
        }

        var uploadBlobModel = new AppendBlobModel
        {
            BlobContent = command.FileContent,
            BlobName = fileName,
            CreateOptions = new AppendBlobCreateOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "TraceId", traceId.ToString() },
                    { "EtlTaskDocumentId", documentId.ToString() }
                }
            }
        };

        await _storageService.AppendAsync(uploadBlobModel, cancellationToken);

        return new Result<EtlTaskDocument> { Value = lawToIngestDocument };
    }    
}

/*
         * using var memoryStream = new MemoryStream();

        await blobDownloadInfo.Value.Content.CopyToAsync(memoryStream, cancellationToken);

        return memoryStream.ToArray();*/