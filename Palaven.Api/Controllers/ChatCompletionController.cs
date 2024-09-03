using Microsoft.AspNetCore.Mvc;
using Palaven.Api.Model.ChatCompletion;
using Palaven.Chat.Contracts;
using Palaven.Model.Chat;

namespace Palaven.Api.Controllers
{
    [Route("api/chatcompletion")]
    [ApiController]
    public class ChatCompletionController : ControllerBase
    {
        private readonly IGemmaChatService _gemmaChatService;

        public ChatCompletionController(IGemmaChatService gemmaChatService)
        {
            _gemmaChatService = gemmaChatService ?? throw new ArgumentNullException(nameof(gemmaChatService));
        }

        [Consumes("multipart/form-data")]
        [HttpPost("{largeLanguageModel}/prompt/augment")]
        public async Task<IActionResult> AugmentQuery([FromRoute] string largeLanguageModel, [FromForm] QueryAugmentationModel inputModel)
        {
            if(!string.Equals(largeLanguageModel, "google-gemma", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"The LLM {largeLanguageModel} is not supported for this operation.");
            }

            var command = new CreateAugmentedQueryPromptCommand
            {
                Query = inputModel.Query,
                TopK = inputModel.TopK,
                MinMatchScore = inputModel.MinMatchScore,
                UserId = Guid.NewGuid()
            };

            var chatMessage = await _gemmaChatService.CreateAugmentedQueryPromptAsync(command, CancellationToken.None);

            return Ok(chatMessage);
        }

        [Consumes("multipart/form-data")]
        [HttpPost("{largeLanguageModel}/prompt")]
        public IActionResult Prompt([FromRoute] string largeLanguageModel, [FromForm] QueryAugmentationModel inputModel)
        {
            if(!string.Equals(largeLanguageModel, "google-gemma", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"The LLM {largeLanguageModel} is not supported for this operation.");
            }

            var query = new ChatMessage
            {
                Prompt = inputModel.Query,
                UserId = Guid.NewGuid().ToString()
            };

            var chatMessage = _gemmaChatService.CreateSimpleQueryPrompt(query);

            return Ok(chatMessage);
        }
    }
}
