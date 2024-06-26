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

            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            
            var appConfigurationConnectionString = builder.Configuration.GetConnectionString("AppConfiguration");
            
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(appConfigurationConnectionString).Select(KeyFilter.Any);
                options.ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(new DefaultAzureCredential());
                });
            });

            builder.Services.AddAIServices();
            builder.Services.AddDataServices();
            builder.Services.AddDataSqlServices();
            builder.Services.AddChatServices();
            builder.Services.AddPalavenCoreServices();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
