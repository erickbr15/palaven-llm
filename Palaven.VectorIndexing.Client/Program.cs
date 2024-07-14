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

var service = host.Services.GetRequiredService<IDatasetInstructionService>();

var traceId = new Guid("690cd4bc-1572-4cb9-8deb-04c1a96433d6");
service.CreateInstructionDatasetAsync(traceId, Guid.NewGuid(), CancellationToken.None).Wait();

host.Run();
