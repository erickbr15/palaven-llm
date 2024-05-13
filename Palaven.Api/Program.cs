using Liara.Common.Http;
using Liara.CosmosDb;
using Liara.OpenAI;
using Liara.Pinecone;
using Palaven.Chat.Contracts;
using Palaven.Chat;
using Palaven.Api;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Palaven.Data;
using Palaven.Model.Ingest.Documents.Golden;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((hostingContext, configBuilder) =>
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
}); 

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<CosmosDbConnectionOptions>().BindConfiguration("CosmosDB");
builder.Services.AddOptions<OpenAiOptions>().BindConfiguration("OpenAi");
builder.Services.AddOptions<PineconeOptions>().BindConfiguration("Pinecone");

builder.Services.AddSingleton<IDocumentRepository<TaxLawDocumentGoldenArticle>, TaxLawGoldenArticleDocumentRepository>();

builder.Services.AddSingleton<IHttpProxy, HttpProxy>();
builder.Services.AddSingleton<IOpenAiServiceClient, OpenAiServiceClient>();
builder.Services.AddSingleton<IPineconeServiceClient, PineconeServiceClient>();
builder.Services.AddSingleton<IGemmaQueryAugmentationService, GemmaQueryAugmentationService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();


app.MapPost("/palaven/llm/augmentquery", async (AugmentQueryModel model, IGemmaQueryAugmentationService augmentationQueryService) =>
{
    var augmentedQuery = await augmentationQueryService.AugmentQueryAsync(new Palaven.Model.Chat.ChatMessage
    {
        Query = model.Query,
        UserId = model.UserId
    }, cancellationToken: default);

    return Results.Ok(augmentedQuery);
})
.WithName("PostAugmentQuery");

app.Run();