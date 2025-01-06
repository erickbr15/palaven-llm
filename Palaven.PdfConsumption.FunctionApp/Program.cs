using Azure.Identity;
using Liara.Common.Extensions;
using Liara.Integrations.Azure;
using Liara.Integrations.Extensions;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Palaven.Application.Extensions;
using Palaven.Data.Sql.Extensions;
using Palaven.Infrastructure.MicrosoftAzure.Extensions;
using Palaven.Infrastructure.VectorIndexing.Extensions;
using Palaven.Persistence.CosmosDB.Extensions;
using Palaven.VectorIndexing.Extensions;

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

        var palavenSqlDBConnectionString = hostContext.Configuration.GetConnectionString("SqlDB");
        services.AddDataSqlServices(palavenSqlDBConnectionString!);
        
        services.AddVectorIndexingServices();
        services.AddPalavenVectorIndexingServices();
        services.AddDatasetManagementServices();

        services.AddLogging();        
    })
    .Build();

host.Run();
