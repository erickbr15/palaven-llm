using Liara.Common.Abstractions.Cqrs;
using Microsoft.Extensions.DependencyInjection;
using Palaven.Application.VectorIndexing.Services;
using Palaven.VectorIndexing.Commands;
using Palaven.Application.Model.VectorIndexing;
using Palaven.Application.Abstractions.VectorIndexing;

namespace Palaven.VectorIndexing.Extensions;

public static class ApplicationRootExtensions
{    
    public static void AddPalavenVectorIndexingServices(this IServiceCollection services)
    {
        services.AddSingleton<ICommandHandler<IndexInstructionsCommand, InstructionsIndexingResult>, IndexInstructionsCommandHandler>();
        services.AddSingleton<IInstructionsIndexingChoreographyService, InstructionsIndexingChoreographyService>();
    }            
}
