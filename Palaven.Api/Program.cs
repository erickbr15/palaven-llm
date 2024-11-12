using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Palaven.Infrastructure.Llm.Extensions;

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

            builder.Services.AddLlmServices();

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
