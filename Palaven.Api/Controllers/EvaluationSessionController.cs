using Microsoft.AspNetCore.Mvc;
using Palaven.Api.Model.EvaluationSession;
using Palaven.Core.Datasets;
using Palaven.Core.PerformanceEvaluation;
using Palaven.Model.Datasets;
using Palaven.Model.PerformanceEvaluation;

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
        public async Task<IActionResult> GetEvaluationSessionAsync(Guid id, CancellationToken cancellationToken)
        {            
            var evaluationSession = await _performanceEvaluationService.GetEvaluationSessionAsync(id, cancellationToken);
            return Ok(evaluationSession);            
        }

        [HttpGet("active/dataset/{datasetId}")]
        public IActionResult GetActiveEvaluationSessionByDataset(Guid datasetId)
        {
            var evaluationSession =  _performanceEvaluationService.GetActiveEvaluationSessionByDataset(datasetId);
            if (evaluationSession == null)
            {
                return NotFound();
            }

            return Ok(evaluationSession);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvaluationSessionAsync([FromBody] CreateEvaluationSessionCommand inputModel, CancellationToken cancellationToken)
        {
            var creationResult = await _performanceEvaluationService.CreateEvaluationSessionAsync(inputModel, cancellationToken);
            if(!creationResult.IsSuccess)
            {
                return BadRequest(creationResult);
            }

            var createdEvaluationSession = creationResult.Value;

            return CreatedAtAction(nameof(GetEvaluationSessionAsync), new { id = createdEvaluationSession.SessionId }, createdEvaluationSession);
        }

        [HttpGet("{id}/dataset/instructions")]
        public async Task<IActionResult> GetInstructionsDatasetAsync([FromRoute]Guid id, [FromQuery]int? batchNumber, CancellationToken cancellationToken)
        {
            var queryModel = new FetchInstructionsDataset { SessionId = id, BatchNumber = batchNumber ?? 1 };

            var queryResult = await _datasetInstructionService.FetchInstructionsDatasetAsync(queryModel, cancellationToken);

            if(!queryResult.IsSuccess)
            {
                return BadRequest(queryResult);
            }

            var instructions = queryResult.Value;

            return Ok(instructions);
        }

        [Consumes("multipart/form-data")]
        [HttpPost("{id}/chatcompletion/{evaluationExercise}")]        
        public async Task<IActionResult> UpsertChatCompletionResponseAsync([FromRoute]Guid id, [FromRoute]string evaluationExercise, [FromForm] ChatCompletionResponseModel inputModel, CancellationToken cancellationToken)
        {                                    
            if(inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            if(!ChatCompletionExcerciseType.IsValid(evaluationExercise))
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

            if(!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }        

        [HttpGet("{id}/chatcompletion/{evaluationExercise}/responses")]
        public IActionResult GetChatCompletionLlmResponses([FromRoute] Guid id, [FromRoute]string evaluationExercise, [FromQuery]int? batchNumber)
        {
            if (!ChatCompletionExcerciseType.IsValid(evaluationExercise))
            {
                return BadRequest($"Invalid evaluation exercise {evaluationExercise}");
            }

            var responses = _performanceEvaluationService.FetchChatCompletionLlmResponses(id, batchNumber ?? 1, evaluationExercise.ToLower());

            return Ok(responses);
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
                BertScoreF1 = inputModel.BertScoreF1
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