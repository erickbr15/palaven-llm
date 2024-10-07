using System;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Palaven.Etl.FunctionApp
{
    public class SilverDocumentCreationFunction
    {
        private readonly ILogger<SilverDocumentCreationFunction> _logger;

        public SilverDocumentCreationFunction(ILogger<SilverDocumentCreationFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(SilverDocumentCreationFunction))]
        public void Run([QueueTrigger("silverdocqueue", Connection = "")] QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
        }
    }
}
