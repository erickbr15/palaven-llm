using Azure.Identity;
using Liara.Azure.AI;
using Liara.Azure.BlobStorage;
using Liara.Azure.Extensions;
using Liara.Clients.Extensions;
using Liara.Clients.OpenAI;
using Liara.Clients.Pinecone;
using Liara.Common;
using Liara.Common.Extensions;
using Liara.Common.Http;
using Liara.CosmosDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Palaven.Data.Extensions;
using Palaven.Data.NoSql;
using Palaven.Ingest.Extensions;
using Palaven.Model;
using Palaven.Model.Data.Documents;
using Palaven.Model.Ingest;

var hostBuilder = new HostBuilder()
    .ConfigureAppConfiguration((hostingContext, configBuilder) =>
    {
        var appConfigurationEndpoint = Environment.GetEnvironmentVariable("AppConfigurationEndpoint");

        configBuilder.AddAzureAppConfiguration(options =>
        {
            var azureCredentialOptions = new DefaultAzureCredentialOptions();
            var credentials = new DefaultAzureCredential(azureCredentialOptions);

            options.Connect(new Uri(appConfigurationEndpoint!), credentials).Select(KeyFilter.Any);
            options.ConfigureKeyVault(kv =>
            {
                kv.SetCredential(new DefaultAzureCredential());
            });
        });
    })
    .ConfigureServices((hostContext, services) =>
    {
        var palavenCosmosOptions = new PalavenCosmosOptions();
        hostContext.Configuration.Bind("CosmosDB", palavenCosmosOptions);

        services.AddLiaraCommonServices();
        services.AddLiaraAzureServices(hostContext.Configuration);
        services.AddLiaraOpenAIServices();
        services.AddLiaraPineconeServices();
        services.AddNoSqlDataServices(palavenCosmosOptions);
        services.AddIngestCommands();
    });

var host = hostBuilder.Build();

var options = host.Services.GetRequiredService<IOptions<BlobStorageOptions>>();



// * CREATES THE BRONZE DOCUMENT
//var lawDocumentFilePath = @"C:\github-code\palaven-sat\law-documents\v1\LISR-2024.pdf";
//var fileInfo = new FileInfo(lawDocumentFilePath);
//var fileContent = File.ReadAllBytes(fileInfo.FullName);
//var fileName = fileInfo.Name;

//var model = new IngestLawDocumentModel
//{
//    Acronym = "LISR",
//    LawDocumentVersion = "v1.plain-2024",
//    Year = 2024,
//    Name = "Ley del Impuesto Sobre la Renta",
//    FileContent = fileContent,
//    FileName = fileName,
//    FileExtension = fileInfo.Extension,
//    StartPageExtractionData = 1,
//    ChunkSizeExtractionData = 12,
//    TotalNumberOfPages = 312    
//};

//var ingestService = host.Services.GetRequiredService<IIngestTaxLawDocumentService>();
//var result = ingestService.IngestTaxLawDocumentAsync(model, CancellationToken.None).GetAwaiter().GetResult();

/*
var pageDocumentExtractor = host.Services.GetRequiredService<ITraceableCommand<ExtractLawDocumentPagesModel, IngestLawDocumentTaskInfo>>();
var result = pageDocumentExtractor.ExecuteAsync(new Guid("690cd4bc-1572-4cb9-8deb-04c1a96433d6"), 
    new ExtractLawDocumentPagesModel { OperationId = new Guid("690cd4bc-1572-4cb9-8deb-04c1a96433d6") }, CancellationToken.None).GetAwaiter().GetResult();
*/


// * CREATES THE SILVER DOCUMENT
/*
var extractCommand = host.Services.GetRequiredService<ITraceableCommand<ExtractLawDocumentArticlesModel, IngestLawDocumentTaskInfo>>();

var traceId = new Guid("690cd4bc-1572-4cb9-8deb-04c1a96433d6");
var model = new ExtractLawDocumentArticlesModel { OperationId = traceId };
var result = extractCommand.ExecuteAsync(traceId, model, CancellationToken.None).GetAwaiter().GetResult();
*/


host.Run();
