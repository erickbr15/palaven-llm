using Azure.Identity;
using Liara.Azure.Extensions;
using Liara.Azure.Storage;
using Liara.Clients.Extensions;
using Liara.Common.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Palaven.Data.Extensions;
using Palaven.Data.NoSql;
using Palaven.Ingest.Extensions;

var host = new HostBuilder()    
    .ConfigureFunctionsWebApplication()
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

        services.AddOptions<AzureStorageOptions>().BindConfiguration("AzureStorage");

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddLiaraCommonServices();
        services.AddLiaraAzureServices(hostContext.Configuration);
        services.AddLiaraOpenAIServices();
        services.AddLiaraPineconeServices();
        services.AddNoSqlDataServices(palavenCosmosOptions);
        services.AddIngestCommands();
    })
    .Build();

host.Run();
