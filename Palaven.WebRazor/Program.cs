using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Liara.Common.Extensions;
using Liara.Integrations.Azure;
using Palaven.Application.Ingest.Extensions;
using Palaven.Infrastructure.MicrosoftAzure.Extensions;
using Palaven.Persistence.CosmosDB.Extensions;
using Liara.Integrations.Extensions;


var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

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
//builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
            .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddDistributedTokenCaches();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

// Add services to the container.
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddLiaraCommonServices();
builder.Services.AddLiaraOpenAIServices();
builder.Services.AddAzureAIServices(builder.Configuration);
builder.Services.AddAzureStorageServices(builder.Configuration);

var palavenDBConfig = builder.Configuration.GetSection("CosmosDB:Containers");
var palavenDBConnectionString = builder.Configuration.GetConnectionString("PalavenCosmosDB");

builder.Services.AddNoSqlDataServices(palavenDBConnectionString!, null, palavenDBConfig.Get<Dictionary<string, CosmosDBContainerOptions>>());

builder.Services.AddIngestServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
