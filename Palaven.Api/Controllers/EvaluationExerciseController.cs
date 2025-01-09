using Microsoft.AspNetCore.Mvc;
using Palaven.Api.Model.EvaluationSession;
using Palaven.Application.Model.PerformanceEvaluation;
using Palaven.Application.PerformanceEvaluation;
using Palaven.Infrastructure.Model.PerformanceEvaluation;

namespace Palaven.Api.Controllers
{
    [Route("api/evaluationsession")]
    [ApiController]
    public class EvaluationExerciseController : ControllerBase
    {
        private readonly IPerformanceEvaluationService _performanceEvaluationService;

        public EvaluationExerciseController(IPerformanceEvaluationService performanceEvaluationService)
        {
            _performanceEvaluationService = performanceEvaluationService ?? throw new ArgumentNullException(nameof(performanceEvaluationService));
        }

        [Consumes("multipart/form-data")]
        [HttpPost("{id}/{evaluationExercise}/llmcompletion")]
        public async Task<IActionResult> UpsertChatCompletionResponseAsync([FromRoute] Guid id, [FromRoute] string evaluationExercise, [FromForm] ChatCompletionResponseModel inputModel, CancellationToken cancellationToken)
        {
            if (inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            if (!ChatCompletionExcerciseType.IsValid(evaluationExercise))
            {
                return BadRequest($"Invalid evaluation exercise {evaluationExercise}");
            }

            var response = new ChatCompletionResponse
            {
                BatchNumber = inputModel.BatchNumber,
                InstructionId = inputModel.InstructionId,
                ResponseCompletion = inputModel.ResponseCompletion,
                ElapsedTime = inputModel.ElapsedTime,
                SessionId = id,
                EvaluationExercise = evaluationExercise.ToLower()
            };

            var command = new UpsertChatCompletionResponseCommand { ChatCompletionResponses = new List<ChatCompletionResponse> { response } };

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(command, cancellationToken);

            if (!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpGet("{id}/{evaluationExercise}/llmcompletions")]
        public IActionResult GetChatCompletionLlmResponses([FromRoute] Guid id, [FromRoute] string evaluationExercise, [FromQuery] int? batchNumber)
        {
            if (!ChatCompletionExcerciseType.IsValid(evaluationExercise))
            {
                return BadRequest($"Invalid evaluation exercise {evaluationExercise}");
            }

            var responses = _performanceEvaluationService.FetchChatCompletionLlmResponses(id, batchNumber ?? 1, evaluationExercise.ToLower());

            return Ok(responses);
        }

        [HttpGet("{id}/{evaluationExercise}/instructions/testing")]
        public IActionResult GetChatCompletionLlmInstructions([FromRoute] Guid id, [FromRoute] string evaluationExercise, [FromQuery] int? batchNumber)
        {
            if (!ChatCompletionExcerciseType.IsValid(evaluationExercise))
            {
                return BadRequest($"Invalid evaluation exercise {evaluationExercise}");
            }

            var instructions = _performanceEvaluationService.FetchChatCompletionLlmInstructions(id, batchNumber ?? 1, evaluationExercise.ToLower());

            return Ok(instructions);
        }
    }
}
