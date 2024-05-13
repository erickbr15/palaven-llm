using Microsoft.Extensions.DependencyInjection;

namespace Palaven.Instructions.Extensions;

public static class ApplicationRootExtensions
{
    public static IServiceCollection AddInstructionsServices(this IServiceCollection services)
    {
        services.AddTransient<IDatasetInstructionService, DatasetInstructionService>();
        return services;
    }
}
