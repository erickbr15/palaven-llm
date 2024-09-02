using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using Palaven.Data.Extensions;
using Palaven.Data.Sql.Extensions;
using Palaven.VectorIndexing.Extensions;
using Palaven.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Palaven.Core.Datasets;
using Palaven.Core.PerformanceEvaluation;

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
        services.AddAIServices();
        services.AddDataServices();
        
        var sqlConnectionString = hostContext.Configuration.GetValue<string>("SqlDB:ConnectionString");
        services.AddDataSqlServices(sqlConnectionString!);

        services.AddVectorIndexingServices();
        services.AddPalavenCoreServices();
    });

var host = hostBuilder.Build();

/*
var datasetService = host.Services.GetRequiredService<IFineTuningDatasetService>();

await datasetService.CreateFineTuningPromptDatasetAsync(new Palaven.Model.PerformanceEvaluation.CreateFineTuningDataset
{
    DatasetId = new Guid("F0444B12-5485-4299-B03B-3BDB6D4A2578"),
    LargeLanguageModel = "google-gemma-7b",
}, CancellationToken.None);*/

var performanceEvaluationService = host.Services.GetRequiredService<IPerformanceEvaluationService>();
await performanceEvaluationService.CleanChatCompletionResponseAsync(new Palaven.Model.PerformanceEvaluation.CleanChatCompletionResponseCommand
{
    SessionId = new Guid("EB9C5839-7B20-4D7D-B3F7-17528180676D"),
    BatchNumber = 1,
    ChatCompletionExcerciseType = "llmvanilla"
}, CancellationToken.None);

await host.RunAsync();
