using Azure.Identity;
using Liara.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Palaven.Data.Extensions;
using Palaven.Ingest.Commands;
using Palaven.Ingest.Extensions;
using Palaven.Ingest.Services;
using Palaven.Model.Ingest.Commands;

var hostBuilder = new HostBuilder()
    .ConfigureAppConfiguration((hostingContext, configBuilder) =>
    {
        configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        var config = configBuilder.Build();
        var appConfigurationConnectionString = config.GetConnectionString("AppConfiguration");

        configBuilder.AddAzureAppConfiguration(options =>
        {
            options.Connect(appConfigurationConnectionString).Select(KeyFilter.Any);
            options.ConfigureKeyVault(kv =>
            {
                kv.SetCredential(new DefaultAzureCredential());
            });
        });
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddAIServices();
        services.AddDataServices();
        services.AddIngestServices();        
    });

var host = hostBuilder.Build();


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


/*
 * CREATES THE GOLDEN DOCUMENT */
var service = host.Services.GetRequiredService<IIngestTaxLawDocumentService>();

var traceId = new Guid("690cd4bc-1572-4cb9-8deb-04c1a96433d6");
var lawId = new Guid("de787ea0-6897-4b6c-84a8-753a8534f550");

service.CreateGoldenDocumentsAsync(traceId, lawId, chunkSize: 220, CancellationToken.None).GetAwaiter().GetResult();

host.Run();
