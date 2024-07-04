using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Palaven.Chat.Extensions;
using Palaven.Data.Extensions;
using Palaven.Data.Sql.Extensions;
using Palaven.Core.Extensions;

namespace Palaven.Api
{
    public class Program
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
            
            builder.Services.AddAIServices();
            builder.Services.AddDataServices();

            var sqlConnectionString = builder.Configuration.GetValue<string>("SqlDB:ConnectionString");
            builder.Services.AddDataSqlServices(sqlConnectionString!);

            builder.Services.AddChatServices();
            builder.Services.AddPalavenCoreServices();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            
            builder.Services.AddSwaggerGen();

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
