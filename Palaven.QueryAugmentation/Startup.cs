using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Palaven.QueryAugmentation.Startup))]

namespace Palaven.QueryAugmentation;
public class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        builder.ConfigurationBuilder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    }

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddAIServices();
        builder.Services.AddDataServices();
        builder.Services.AddChatServices();
    }

    
}
