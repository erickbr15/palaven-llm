using Azure.Identity;
using Liara.Common.Extensions;
using Liara.Integrations.Azure;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Palaven.Application.Extensions;
using Palaven.Data.Sql.Extensions;
using Palaven.Infrastructure.Llm.Extensions;
using Palaven.Infrastructure.MicrosoftAzure.Extensions;
using Palaven.Persistence.CosmosDB.Extensions;

namespace Palaven.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
                                    
            var appConfigurationEndpoint = Environment.GetEnvironmentVariable("AppConfigurationEndpoint");
            
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                var azureCredentialOptions = new DefaultAzureCredentialOptions();
                var credentials = new DefaultAzureCredential(azureCredentialOptions);
                
                options.Connect(new Uri(appConfigurationEndpoint!), credentials).Select(KeyFilter.Any);
                options.ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(new DefaultAzureCredential());
                });
            });

            var sqlConnectionString = builder.Configuration.GetConnectionString("SqlDB");            
            builder.Services.AddDataSqlServices(sqlConnectionString!);

            var cosmosContainerConfig = builder.Configuration.GetSection("CosmosDB:Containers");
            var cosmosConnectionString = builder.Configuration.GetConnectionString("PalavenCosmosDB");
            builder.Services.AddNoSqlDataServices(cosmosConnectionString!, null, cosmosContainerConfig.Get<Dictionary<string, CosmosDBContainerOptions>>());

            builder.Services.AddLlmServices();
            builder.Services.AddAzureStorageServices(builder.Configuration);

            builder.Services.AddDatasetManagementServices();
            builder.Services.AddPerformanceEvaluationServices();
            builder.Services.AddPerformanceMetricsService();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            
            builder.Services.AddSwaggerGen();
            builder.Services.ConfigureSwaggerGen(setup =>
            {
                setup.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Palaven LLM - Training & Eval API",
                    Version = "v1"
                });
            });

            var app = builder.Build();

            app.UseSwagger();
            if (app.Environment.IsDevelopment())
            {                
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
