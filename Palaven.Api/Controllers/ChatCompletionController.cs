using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("{largeLanguageModel}/augmentquery")]
        public async Task<IActionResult> AugmentQuery([FromRoute] string largeLanguageModel, [FromBody] ChatMessage query)
        {
            if(!string.Equals(largeLanguageModel, "google-gemma-7b", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"The LLM {largeLanguageModel} is not supported for this operation.");
            }

            var chatMessage = await _gemmaChatService.CreateAugmentedQueryPromptAsync(query, CancellationToken.None);

            return Ok(chatMessage);
        }

        [HttpPost("{largeLanguageModel}/prompt")]
        public IActionResult Prompt([FromRoute] string largeLanguageModel, [FromBody] ChatMessage query)
        {
            if(!string.Equals(largeLanguageModel, "google-gemma-7b", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"The LLM {largeLanguageModel} is not supported for this operation.");
            }

            var chatMessage = _gemmaChatService.CreateSimpleQueryPrompt(query);
            return Ok(chatMessage);
        }
    }
}
