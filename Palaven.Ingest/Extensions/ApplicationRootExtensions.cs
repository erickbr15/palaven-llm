using Microsoft.Extensions.DependencyInjection;
using Palaven.Ingest.Commands;
using Liara.Common;
using Palaven.Model.Ingest;
using Palaven.Model.Data.Documents;
using Palaven.Model;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Palaven.Ingest.Extensions;

public static class ApplicationRootExtensions
{    
    public static void AddIngestCommands(this IServiceCollection services)
    {
        services.AddOptions<BlobStorageOptions>().BindConfiguration("BlobStorage");
        services.AddSingleton<ICommandHandler<StartTaxLawIngestCommand, EtlTaskDocument>, StartTaxLawIngestCommandHandler>();

        /*services.AddSingleton<ICommandHandler<CreateBronzeDocumentCommand, TaxLawDocumentIngestTask>, CreateBronzeDocumentCommandHandler>();
        services.AddSingleton<ICommandHandler<CreateSilverDocumentCommand, TaxLawDocumentIngestTask>, CreateSilverDocumentCommandHandler>();
        services.AddSingleton<ICommandHandler<CreateGoldenDocumentCommand, TaxLawDocumentIngestTask>, CreateGoldenDocumentCommandHandler>();*/
    }

    public static void AddIngestServices(this IServiceCollection services)
    {               
        services.AddSingleton<IIngestTaxLawDocumentService, IngestTaxLawDocumentService>();
    }
}
