using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Palaven.Data.NoSql;
using Palaven.Data.Extensions;
using Palaven.Ingest.Extensions;
using Liara.Common.Extensions;
using Liara.Azure.Extensions;
using Liara.Azure.Storage;

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

var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
            .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

// Add services to the container.
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

var palavenCosmosOptions = new PalavenCosmosOptions();
builder.Configuration.Bind("CosmosDB", palavenCosmosOptions);


builder.Services.AddOptions<AzureStorageOptions>().BindConfiguration("AzureStorage");
builder.Services.AddLiaraCommonServices();
builder.Services.AddLiaraAzureServices(builder.Configuration);
builder.Services.AddNoSqlDataServices(palavenCosmosOptions);
builder.Services.AddIngestCommands();

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
