using Microsoft.AspNetCore.Mvc;
using Palaven.Core.Datasets;
using Palaven.Core.PerformanceEvaluation;
using Palaven.Model.PerformanceEvaluation;
using Palaven.Model.PerformanceEvaluation.Commands;
using Palaven.Model.PerformanceEvaluation.Web;

namespace Palaven.Api.Controllers
{
    [Route("api/evaluationsession")]
    [ApiController]
    public class EvaluationSessionController : ControllerBase
    {
        private readonly IPerformanceEvaluationService _performanceEvaluationService;
        private readonly IDatasetInstructionService _datasetInstructionService;

        public EvaluationSessionController(IPerformanceEvaluationService performanceEvaluationService, IDatasetInstructionService datasetInstructionService)
        {
            _performanceEvaluationService = performanceEvaluationService ?? throw new ArgumentNullException(nameof(performanceEvaluationService));
            _datasetInstructionService = datasetInstructionService ?? throw new ArgumentNullException(nameof(datasetInstructionService));
        }        

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvaluationSession(Guid id)
        {            
            var evaluationSession = await _performanceEvaluationService.GetEvaluationSessionAsync(id, CancellationToken.None);            
            return Ok(evaluationSession);            
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvaluationSession([FromBody] CreateEvaluationSessionModel inputModel)
        {
            var creationResult = await _performanceEvaluationService.CreateEvaluationSessionAsync(inputModel, CancellationToken.None);            
            if(creationResult.AnyErrorsOrValidationFailures)
            {
                return BadRequest(creationResult);
            }

            var createdEvaluationSession = creationResult.Value;

            return CreatedAtAction(nameof(GetEvaluationSession), new { id = createdEvaluationSession.SessionId }, createdEvaluationSession);
        }

        [HttpGet("{id}/dataset/instructions")]
        public async Task<IActionResult> GetInstructionsDataset([FromRoute]Guid id, [FromQuery]int batchNumber)
        {
            var queryModel = new QueryInstructionsDatasetModel { SessionId = id, BatchNumber = batchNumber };
            var queryResult = await _datasetInstructionService.FetchInstructionsDatasetAsync(queryModel, CancellationToken.None);

            if(queryResult.AnyErrorsOrValidationFailures)
            {
                return BadRequest(queryResult);
            }

            var instructions = queryResult.Value;

            return Ok(instructions);
        }

        [HttpPost("{id}/chatcompletion/vanilla/response")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpsertVanillaChatCompletionResponse([FromRoute]Guid id, [FromForm] ChatCompletionResponseModel inputModel)
        {                                    
            if(inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            var upsertModel = new UpsertChatCompletionResponseModel
            {
                BatchNumber = inputModel.BatchNumber,
                InstructionId = inputModel.InstructionId,
                ResponseCompletion = inputModel.ResponseCompletion,
                ElapsedTime = inputModel.ElapsedTime,
                SessionId = id,
                ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmVanilla
            };            
            
            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(
                new List<UpsertChatCompletionResponseModel> { upsertModel }, 
                CancellationToken.None);

            if(upsertResult.AnyErrorsOrValidationFailures)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpPost("{id}/chatcompletion/vanilla/response/bulk")]
        public async Task<IActionResult> UpsertVanillaChatCompletionBulkResponse([FromRoute] Guid id, [FromBody] IEnumerable<UpsertChatCompletionResponseModel> inputModel)
        {
            if (inputModel == null || !inputModel.Any())
            {
                return BadRequest("Input model is required");
            }
            
            var responses = inputModel.ToList();
            
            responses.ForEach(r => {
                r.SessionId = id;
                r.ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmVanilla;
            });             

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(responses,CancellationToken.None);

            if (upsertResult.AnyErrorsOrValidationFailures)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpPost("{id}/chatcompletion/rag/response")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpsertRagChatCompletionResponse([FromRoute] Guid id, [FromForm] ChatCompletionResponseModel inputModel)
        {
            if (inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            var upsertModel = new UpsertChatCompletionResponseModel
            {
                BatchNumber = inputModel.BatchNumber,
                InstructionId = inputModel.InstructionId,
                ResponseCompletion = inputModel.ResponseCompletion,
                ElapsedTime = inputModel.ElapsedTime,
                SessionId = id,
                ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmWithRag
            };            

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(
                new List<UpsertChatCompletionResponseModel> { upsertModel },
                CancellationToken.None);

            if (upsertResult.AnyErrorsOrValidationFailures)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpPost("{id}/chatcompletion/rag/response/bulk")]
        public async Task<IActionResult> UpsertRagChatCompletionBulkResponse([FromRoute] Guid id, [FromBody] IEnumerable<UpsertChatCompletionResponseModel> inputModel)
        {
            if (inputModel == null || !inputModel.Any())
            {
                return BadRequest("Input model is required");
            }

            var responses = inputModel.ToList();

            responses.ForEach(r => {
                r.SessionId = id;
                r.ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmWithRag;
            });

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(responses, CancellationToken.None);

            if (upsertResult.AnyErrorsOrValidationFailures)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpPost("{id}/chatcompletion/finetuned/response")]
        public async Task<IActionResult> UpsertFinetunedChatCompletionResponse([FromRoute] Guid id, [FromBody] UpsertChatCompletionResponseModel inputModel)
        {
            if (inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            inputModel.SessionId = id;
            inputModel.ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmFineTuned;

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(
                new List<UpsertChatCompletionResponseModel> { inputModel },
                CancellationToken.None);

            if (upsertResult.AnyErrorsOrValidationFailures)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpPost("{id}/chatcompletion/finetuned/response/bulk")]
        public async Task<IActionResult> UpsertFinetunedChatCompletionBulkResponse([FromRoute] Guid id, [FromBody] IEnumerable<UpsertChatCompletionResponseModel> inputModel)
        {
            if (inputModel == null || !inputModel.Any())
            {
                return BadRequest("Input model is required");
            }

            var responses = inputModel.ToList();

            responses.ForEach(r => {
                r.SessionId = id;
                r.ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmFineTuned;
            });

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(responses, CancellationToken.None);

            if (upsertResult.AnyErrorsOrValidationFailures)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpPost("{id}/chatcompletion/finetuned-rag/response")]
        public async Task<IActionResult> UpsertFinetunedRagChatCompletionResponse([FromRoute] Guid id, [FromBody] UpsertChatCompletionResponseModel inputModel)
        {
            if (inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            inputModel.SessionId = id;
            inputModel.ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmFineTunedAndRag;

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(
                new List<UpsertChatCompletionResponseModel> { inputModel },
                CancellationToken.None);

            if (upsertResult.AnyErrorsOrValidationFailures)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpPost("{id}/chatcompletion/finetuned-rag/response/bulk")]
        public async Task<IActionResult> UpsertFinetunedRagChatCompletionBulkResponse([FromRoute] Guid id, [FromBody] IEnumerable<UpsertChatCompletionResponseModel> inputModel)
        {
            if (inputModel == null || !inputModel.Any())
            {
                return BadRequest("Input model is required");
            }

            var responses = inputModel.ToList();

            responses.ForEach(r => {
                r.SessionId = id;
                r.ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmFineTunedAndRag;
            });

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(responses, CancellationToken.None);

            if (upsertResult.AnyErrorsOrValidationFailures)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpPost("{id}/chatcompletion/bertscore-metrics")]
        public async Task<IActionResult> UpsertBertScoreMetrics([FromRoute] Guid id, [FromBody] UpsertChatCompletionPerformanceEvaluationModel inputModel)
        {
            if (inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            inputModel.SessionId = id;
            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionPerformanceEvaluationAsync(inputModel, CancellationToken.None);

            if (upsertResult.AnyErrorsOrValidationFailures)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

    }
}