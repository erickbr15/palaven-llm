using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Palaven.Chat.Contracts;
using Palaven.Model.Chat;

namespace Palaven.QueryAugmentation.Function;

public class QueryAugmentationFunction
{
    private readonly ILogger<QueryAugmentationFunction> _logger;
    private readonly IGemmaQueryAugmentationService _gemmaQueryAugmentationService;

    public QueryAugmentationFunction(ILogger<QueryAugmentationFunction> logger, IGemmaQueryAugmentationService gemmaQueryAugmentationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gemmaQueryAugmentationService = gemmaQueryAugmentationService ?? throw new ArgumentNullException(nameof(gemmaQueryAugmentationService));
    }

    [FunctionName(nameof(QueryAugmentationFunction))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "palaven/llm/augmentquery")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");            

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var inputModel = JsonConvert.DeserializeObject<QueryAugmentationModel>(requestBody);

        if(inputModel == null)
        {
            return new BadRequestObjectResult("Please pass a valid request body.");
        }

        var chatMessage = new ChatMessage
        {
            UserId = inputModel.TenantId,
            Query = inputModel.Query
        };

        var augmentedQuery = await _gemmaQueryAugmentationService.AugmentQueryAsync(chatMessage, cancellationToken: default);

        return new OkObjectResult(augmentedQuery);
    }
}
