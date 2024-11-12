using Palaven.Infrastructure.Model.AI.Llm;

namespace Palaven.Infrastructure.Abstractions.AI.Llm;

public interface IRetrievalService
{
    Task<IEnumerable<TDocument>> RetrieveRelatedDocumentsAsync<TDocument>(IEnumerable<string> instructions, RetrievalOptions options, CancellationToken cancellationToken) where TDocument : class;
}
