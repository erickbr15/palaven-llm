using Microsoft.AspNetCore.Mvc;
using Palaven.Core.Datasets;
using Palaven.Core.PerformanceEvaluation;
using Palaven.Model.Datasets;
using Palaven.Model.PerformanceEvaluation;
using Palaven.Model.PerformanceEvaluation.Web;

namespace Palaven.Api.Controllers
{
    [Route("api/evaluationsession")]
    [ApiController]
    public class EvaluationSessionController : ControllerBase
    {
        private readonly IPerformanceEvaluationService _performanceEvaluationService;
        private readonly IInstructionDatasetService _datasetInstructionService;

        public EvaluationSessionController(IPerformanceEvaluationService performanceEvaluationService, IInstructionDatasetService datasetInstructionService)
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
            if(!creationResult.IsSuccess)
            {
                return BadRequest(creationResult);
            }

            var createdEvaluationSession = creationResult.Value;

            return CreatedAtAction(nameof(GetEvaluationSession), new { id = createdEvaluationSession.SessionId }, createdEvaluationSession);
        }

        [HttpGet("{id}/dataset/instructions")]
        public async Task<IActionResult> GetInstructionsDataset([FromRoute]Guid id, [FromQuery]int? batchNumber)
        {
            var queryModel = new FetchInstructionsDataset { SessionId = id, BatchNumber = batchNumber ?? 1 };
            var queryResult = await _datasetInstructionService.FetchInstructionsDatasetAsync(queryModel, CancellationToken.None);

            if(!queryResult.IsSuccess)
            {
                return BadRequest(queryResult);
            }

            var instructions = queryResult.Value;

            return Ok(instructions);
        }

        [Consumes("multipart/form-data")]
        [HttpPost("{id}/chatcompletion/vanilla")]        
        public async Task<IActionResult> UpsertVanillaChatCompletionResponse([FromRoute]Guid id, [FromForm] ChatCompletionResponseModel inputModel)
        {                                    
            if(inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            var response = new ChatCompletionResponse
            {
                BatchNumber = inputModel.BatchNumber,
                InstructionId = inputModel.InstructionId,
                ResponseCompletion = inputModel.ResponseCompletion,
                ElapsedTime = inputModel.ElapsedTime,
                SessionId = id,
                ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmVanilla
            };

            var command = new UpsertChatCompletionResponseCommand { ChatCompletionResponses = new List<ChatCompletionResponse> { response } };

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(command, CancellationToken.None);

            if(!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpPost("{id}/chatcompletion/vanilla/bulk")]
        public async Task<IActionResult> UpsertVanillaChatCompletionBulkResponse([FromRoute] Guid id, [FromBody] IEnumerable<ChatCompletionResponse> inputModel)
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

            var command = new UpsertChatCompletionResponseCommand { ChatCompletionResponses = responses };

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(command, CancellationToken.None);

            if (!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpGet("{id}/chatcompletion/vanilla/responses")]
        public IActionResult GetChatCompletionLlmResponses([FromRoute] Guid id, [FromQuery]int? batchNumber)
        {
            var responses = _performanceEvaluationService.FetchChatCompletionLlmResponses(id, batchNumber ?? 1, ChatCompletionExcerciseType.LlmVanilla);
            return Ok(responses);
        }

        [Consumes("multipart/form-data")]
        [HttpPost("{id}/chatcompletion/rag")]        
        public async Task<IActionResult> UpsertRagChatCompletionResponse([FromRoute] Guid id, [FromForm] ChatCompletionResponseModel inputModel)
        {
            if (inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            var response = new ChatCompletionResponse
            {
                BatchNumber = inputModel.BatchNumber,
                InstructionId = inputModel.InstructionId,
                ResponseCompletion = inputModel.ResponseCompletion,
                ElapsedTime = inputModel.ElapsedTime,
                SessionId = id,
                ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmWithRag
            };

            var command = new UpsertChatCompletionResponseCommand { ChatCompletionResponses = new List<ChatCompletionResponse> { response } };

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(command, CancellationToken.None);

            if (!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpPost("{id}/chatcompletion/rag/bulk")]
        public async Task<IActionResult> UpsertRagChatCompletionBulkResponse([FromRoute] Guid id, [FromBody] IEnumerable<ChatCompletionResponse> inputModel)
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

            var command = new UpsertChatCompletionResponseCommand { ChatCompletionResponses = responses };

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(command, CancellationToken.None);

            if (!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpGet("{id}/chatcompletion/rag/responses")]
        public IActionResult GetChatCompletionLlmRagResponses([FromRoute] Guid id, [FromQuery] int? batchNumber)
        {
            var responses = _performanceEvaluationService.FetchChatCompletionLlmResponses(id, batchNumber ?? 1, ChatCompletionExcerciseType.LlmWithRag);
            return Ok(responses);
        }

        [HttpPost("{id}/chatcompletion/finetuned")]
        public async Task<IActionResult> UpsertFinetunedChatCompletionResponse([FromRoute] Guid id, [FromBody] ChatCompletionResponse inputModel)
        {
            if (inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            inputModel.SessionId = id;
            inputModel.ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmFineTuned;

            var command = new UpsertChatCompletionResponseCommand { ChatCompletionResponses = new List<ChatCompletionResponse> { inputModel } };

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(command, CancellationToken.None);

            if (!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpPost("{id}/chatcompletion/finetuned/bulk")]
        public async Task<IActionResult> UpsertFinetunedChatCompletionBulkResponse([FromRoute] Guid id, [FromBody] IEnumerable<ChatCompletionResponse> inputModel)
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

            var command = new UpsertChatCompletionResponseCommand { ChatCompletionResponses = responses };

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(command, CancellationToken.None);

            if (!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpPost("{id}/chatcompletion/finetuned-rag")]
        public async Task<IActionResult> UpsertFinetunedRagChatCompletionResponse([FromRoute] Guid id, [FromBody] ChatCompletionResponse inputModel)
        {
            if (inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            inputModel.SessionId = id;
            inputModel.ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmFineTunedAndRag;

            var command = new UpsertChatCompletionResponseCommand { ChatCompletionResponses = new List<ChatCompletionResponse> { inputModel } };

            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(command, CancellationToken.None);

            if (!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpPost("{id}/chatcompletion/finetuned-rag/bulk")]
        public async Task<IActionResult> UpsertFinetunedRagChatCompletionBulkResponse([FromRoute] Guid id, [FromBody] IEnumerable<ChatCompletionResponse> inputModel)
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

            var command = new UpsertChatCompletionResponseCommand { ChatCompletionResponses = responses };
            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionResponseAsync(command, CancellationToken.None);

            if (!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }        

        [HttpPost("{id}/metrics")]
        public async Task<IActionResult> UpsertBertScoreMetricsAsync([FromRoute] Guid id, [FromBody] ChatCompletionPerformanceEvaluationModel inputModel, CancellationToken cancellationToken)
        {
            if (inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            var upsertModel = new UpsertChatCompletionPerformanceEvaluation
            {
                SessionId = id,
                BatchNumber = inputModel.BatchNumber,                
                BertScorePrecision = inputModel.BertScorePrecision,
                BertScoreRecall = inputModel.BertScoreRecall,
                BertScoreF1 = inputModel.BertScoreF1,
                RougeScoreMetrics = inputModel.RougeScoreMetrics
            };
            
            var upsertResult = await _performanceEvaluationService.UpsertChatCompletionPerformanceEvaluationAsync(upsertModel, cancellationToken);

            if (!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }
    }
}