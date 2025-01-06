using Azure.Identity;
using Liara.Common.Extensions;
using Liara.Integrations.Azure;
using Liara.Integrations.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Palaven.Application.Ingest.Extensions;
using Palaven.Infrastructure.MicrosoftAzure.Extensions;
using Palaven.Persistence.CosmosDB.Extensions;


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
        services.AddLiaraCommonServices();
        services.AddLiaraOpenAIServices();
        services.AddLiaraPineconeServices();

        services.AddAzureAIServices(hostContext.Configuration);
        services.AddAzureStorageServices(hostContext.Configuration);

        var palavenDBConfig = hostContext.Configuration.GetSection("CosmosDB:Containers");
        var palavenDBConnectionString = hostContext.Configuration.GetConnectionString("PalavenCosmosDB");

        services.AddNoSqlDataServices(palavenDBConnectionString!, null, palavenDBConfig.Get<Dictionary<string, CosmosDBContainerOptions>>());
        services.AddIngestServices();
        services.AddLogging();
    })    
    .Build();

host.Run();
