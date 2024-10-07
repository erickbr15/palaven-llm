using System;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Palaven.Etl.FunctionApp
{
    public class GoldenDocumentCreationFunction
    {
        private readonly ILogger<GoldenDocumentCreationFunction> _logger;

        public GoldenDocumentCreationFunction(ILogger<GoldenDocumentCreationFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(GoldenDocumentCreationFunction))]
        public void Run([QueueTrigger("goldendocqueue", Connection = "")] QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
        }
    }
}
