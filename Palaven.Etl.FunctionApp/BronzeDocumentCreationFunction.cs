using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Palaven.Etl.FunctionApp
{
    public class BronzeDocumentCreationFunction
    {
        private readonly ILogger<BronzeDocumentCreationFunction> _logger;

        public BronzeDocumentCreationFunction(ILogger<BronzeDocumentCreationFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(BronzeDocumentCreationFunction))]
        public async Task Run([BlobTrigger("lawdocs-v1/{name}.pdf", Connection = "BlobStorageConnectionstring")] Stream stream, string name)
        {
            using var blobStreamReader = new StreamReader(stream);
            var content = await blobStreamReader.ReadToEndAsync();
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");
        }
    }
}
