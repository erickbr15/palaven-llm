using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using Palaven.Data.Extensions;
using Palaven.Data.Sql.Extensions;
using Palaven.Instructions.Extensions;
using Palaven.VectorIndexing.Extensions;

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
        services.AddDataSqlServices();
        services.AddVectorIndexingServices();
        services.AddInstructionsServices();
    });

var host = hostBuilder.Build();


host.Run();
