using Microsoft.AspNetCore.Mvc;
using Palaven.Api.Model.ChatCompletion;
using Palaven.Infrastructure.Abstractions.AI.Llm;
using Palaven.Infrastructure.Model.AI.Llm;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Api.Controllers
{
    [Route("api/chatcompletion")]
    [ApiController]
    public class ChatCompletionController : ControllerBase
    {
        private readonly IPromptEngineeringService<string> _promptEngineeringService;
        private readonly IRetrievalService _retrievalService;

        public ChatCompletionController(IPromptEngineeringService<string> promptEngineeringService, IRetrievalService retrievalService)
        {
            _promptEngineeringService = promptEngineeringService ?? throw new ArgumentNullException(nameof(promptEngineeringService));
            _retrievalService = retrievalService ?? throw new ArgumentNullException(nameof(retrievalService));
        }

        [Consumes("multipart/form-data")]
        [HttpPost("{largeLanguageModel}/prompt/augment")]
        public async Task<IActionResult> AugmentQuery([FromRoute] string largeLanguageModel, [FromForm] QueryAugmentationModel inputModel)
        {
            if(!string.Equals(largeLanguageModel, "google-gemma", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"The LLM {largeLanguageModel} is not supported for this operation.");
            }

            var retrievalOptions = new RetrievalOptions
            {
                IncludeValues = true,
                MinimumMatchScore = inputModel.MinMatchScore,
                Namespace = "palaven",
                TopK = inputModel.TopK               
            };

            var relatedDocuments = await _retrievalService.RetrieveRelatedDocumentsAsync<GoldenDocument>(new List<string> { inputModel.Query }, retrievalOptions, CancellationToken.None);
            
            var augmentedPrompt = _promptEngineeringService.CreateAugmentedQueryPrompt(inputModel.Query, relatedDocuments);            

            return Ok(augmentedPrompt);
        }

        [Consumes("multipart/form-data")]
        [HttpPost("{largeLanguageModel}/prompt")]
        public IActionResult Prompt([FromRoute] string largeLanguageModel, [FromForm] QueryAugmentationModel inputModel)
        {
            if(!string.Equals(largeLanguageModel, "google-gemma", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"The LLM {largeLanguageModel} is not supported for this operation.");
            }            

            var prompt = _promptEngineeringService.CreateSimpleQueryPrompt(inputModel.Query);

            return Ok(prompt);
        }
    }
}
