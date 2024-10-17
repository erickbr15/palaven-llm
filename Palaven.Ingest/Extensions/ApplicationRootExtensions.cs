using Microsoft.Extensions.DependencyInjection;
using Palaven.Ingest.Commands;
using Liara.Common;
using Palaven.Model.Ingest;
using Palaven.Model.Data.Documents;
using Palaven.Model;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Azure;

namespace Palaven.Ingest.Extensions;

public static class ApplicationRootExtensions
{    
    public static void AddIngestCommands(this IServiceCollection services)
    {        
        services.AddSingleton<ICommandHandler<StartTaxLawIngestCommand, EtlTaskDocument>, StartTaxLawIngestProcessCommandHandler>();
        services.AddSingleton<ICommandHandler<StartBronzeDocumentCommand, string>, StartBronzeDocumentCommandHandler>();
        services.AddSingleton<ICommandHandler<CompleteBronzeDocumentCommand, EtlTaskDocument>, CompleteBronzeDocumentCommandHandler>();

        /*
        services.AddSingleton<ICommandHandler<CreateSilverDocumentCommand, TaxLawDocumentIngestTask>, CreateSilverDocumentCommandHandler>();
        services.AddSingleton<ICommandHandler<CreateGoldenDocumentCommand, TaxLawDocumentIngestTask>, CreateGoldenDocumentCommandHandler>();*/
    }

    public static void AddIngestServices(this IServiceCollection services)
    {               
        services.AddSingleton<IIngestTaxLawDocumentService, IngestTaxLawDocumentService>();
    }
}
