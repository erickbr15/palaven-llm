using Microsoft.Extensions.DependencyInjection;
using Palaven.Infrastructure.Abstractions.VectorIndexing;

namespace Palaven.Infrastructure.VectorIndexing.Extensions;

public static class AppRootExtensions
{
    public static void AddVectorIndexingServices(this IServiceCollection services)
    {
        services.AddSingleton<IVectorIndexService, PineconeVectorIndexService>();
    }
}
