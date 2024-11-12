using Microsoft.Extensions.DependencyInjection;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Model.Persistence.Documents;
using Palaven.Infrastructure.Model.AI;
using Palaven.Application.Ingest.Commands;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Application.Ingest.Services;

namespace Palaven.Application.Ingest.Extensions;

public static class ApplicationRootExtensions
{    
    public static void AddIngestServices(this IServiceCollection services)
    {
        services.AddSingleton<ICommandHandler<StartTaxLawIngestCommand, EtlTaskDocument>, StartTaxLawIngestProcessCommandHandler>();
        services.AddSingleton<ICommandHandler<AnalyzeDocumentCommand, string>, StartDocumentAnalysisCommandHandler>();
        services.AddSingleton<ICommandHandler<GetDocumentAnalysisResultCommand, DocumentAnalysisResult>, GetDocumentAnalysisResultCommandHandler>();
        services.AddSingleton<ICommandHandler<ExtractDocumentPagesCommand, EtlTaskDocument>, ExtractDocumentPagesCommandHandler>();
        services.AddSingleton<ICommandHandler<ExtractArticleParagraphsCommand, EtlTaskDocument>, ExtractArticleParagraphsCommandHandler>();
        services.AddSingleton<ICommandHandler<CurateArticlesCommand, ArticlesCurationResult>, CurateArticlesCommandHandler>();
        services.AddSingleton<ICommandHandler<GenerateInstructionsCommand, InstructionGenerationResult>, GenerateInstructionsCommandHandler>();

        services.AddSingleton<IStartIngestionChoreographyService, StartIngestionChoreographyService>();
        services.AddSingleton<IDocumentAnalysisChoreographyService, DocumentAnalysisChoreographyService>();
        services.AddSingleton<IDocumentPagesExtractionChoreographyService, DocumentPagesExtractionChoreographyService>();
        services.AddSingleton<IArticleParagraphsExtractionChoreographyService, ArticleParagraphsExtractionChoreographyService>();
        services.AddSingleton<IArticlesCurationChoreographyService, ArticlesCurationChoreographyService>();
        services.AddSingleton<IInstructionGenerationChoreographyService, InstructionGenerationChoreographyService>();
    }    
}
