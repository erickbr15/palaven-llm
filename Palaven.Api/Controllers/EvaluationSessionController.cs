using Microsoft.AspNetCore.Mvc;
using Palaven.Api.Model.EvaluationSession;
using Palaven.Application.Abstractions.DatasetManagement;
using Palaven.Application.Abstractions.PerformanceMetrics;
using Palaven.Application.Model.PerformanceEvaluation;
using Palaven.Application.Model.PerformanceMetrics;
using Palaven.Application.PerformanceEvaluation;
using Palaven.Infrastructure.Model.PerformanceEvaluation;
using Palaven.Model.Datasets;

namespace Palaven.Api.Controllers
{
    [Route("api/evaluationsession")]
    [ApiController]
    public class EvaluationSessionController : ControllerBase
    {
        private readonly IPerformanceEvaluationService _performanceEvaluationService;
        private readonly IPerformanceMetricsService _performanceMetricsService;
        private readonly IInstructionDatasetService _datasetInstructionService;

        public EvaluationSessionController(IPerformanceEvaluationService performanceEvaluationService, IPerformanceMetricsService performanceMetricsService, 
            IInstructionDatasetService datasetInstructionService)
        {
            _performanceEvaluationService = performanceEvaluationService ?? throw new ArgumentNullException(nameof(performanceEvaluationService));
            _performanceMetricsService = performanceMetricsService ?? throw new ArgumentNullException(nameof(performanceMetricsService));
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
            var queryModel = new FetchInstructionsDatasetRequest { SessionId = id, BatchNumber = batchNumber ?? 1 };

            var queryResult = await _datasetInstructionService.FetchInstructionsDatasetAsync(queryModel, cancellationToken);

            if(!queryResult.IsSuccess)
            {
                return BadRequest(queryResult);
            }

            var instructions = queryResult.Value;

            return Ok(instructions);
        }

        [Consumes("multipart/form-data")]
        [HttpPost("{id}/chatcompletion/{evaluationExercise}/response")]        
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

        [HttpGet("{id}/chatcompletion/{evaluationExercise}/instructions")]
        public IActionResult GetChatCompletionLlmInstructions([FromRoute] Guid id, [FromRoute] string evaluationExercise, [FromQuery] int? batchNumber)
        {
            if (!ChatCompletionExcerciseType.IsValid(evaluationExercise))
            {
                return BadRequest($"Invalid evaluation exercise {evaluationExercise}");
            }

            var instructions = _performanceEvaluationService.FetchChatCompletionLlmInstructions(id, batchNumber ?? 1, evaluationExercise.ToLower());

            return Ok(instructions);
        }

        [HttpPost("{id}/metrics/{evaluationExercise}/bertscore")]
        public async Task<IActionResult> UpsertBertScoreMetricsAsync([FromRoute] Guid id, [FromRoute] string evaluationExercise, [FromBody] BertScoreBatchEvaluationModel inputModel, CancellationToken cancellationToken)
        {
            if (inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            if (!ChatCompletionExcerciseType.IsValid(evaluationExercise))
            {
                return BadRequest($"Invalid evaluation exercise {evaluationExercise}");
            }

            var command = new UpsertBertScoreBatchEvaluationRequest
            {
                SessionId = id,
                EvaluationExercise = evaluationExercise.ToLower(),
                BatchNumber = inputModel.BatchNumber,                
                Precision = inputModel.Precision,
                Recall = inputModel.Recall,
                F1 = inputModel.F1
            };
            
            var upsertResult = await _performanceMetricsService.UpsertBertscoreBatchEvaluationAsync(command, cancellationToken);

            if (!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpGet("{id}/metrics/{evaluationExercise}/bertscore")]
        public IActionResult GetBertScoreMetrics([FromRoute]Guid id, [FromRoute]string evaluationExercise)
        {
            if (!ChatCompletionExcerciseType.IsValid(evaluationExercise))
            {
                return BadRequest($"Invalid evaluation exercise {evaluationExercise}");
            }

            var metrics = _performanceMetricsService.FetchEvaluationSessionBertscoreMetrics(id, evaluationExercise.ToLower());

            return Ok(metrics);
        }

        [HttpPost("{id}/metrics/{evaluationExercise}/rougescore")]
        public async Task<IActionResult> UpsertRougeScoreMetricsAsync([FromRoute] Guid id, [FromRoute] string evaluationExercise, [FromBody] IList<RougeScoreBatchEvaluationModel> inputModel, CancellationToken cancellationToken)
        {
            if(inputModel == null || inputModel.Count == 0)
            {
                return BadRequest("The rouge score metrics are required");
            }

            if(!ChatCompletionExcerciseType.IsValid(evaluationExercise))
            {
                return BadRequest($"Invalid evaluation exercise {evaluationExercise}");
            }

            var commands = inputModel.Select(inputModel => new UpsertRougeScoreBatchEvaluationRequest
            {
                SessionId = id,
                EvaluationExercise = evaluationExercise.ToLower(),
                BatchNumber = inputModel.BatchNumber,
                RougeType = inputModel.RougeScoreType,
                Precision = inputModel.Precision,
                Recall = inputModel.Recall,
                F1 = inputModel.F1
            }).ToList();

            var upsertResult = await _performanceMetricsService.UpsertRougeScoreBatchEvaluationAsync(commands, cancellationToken);

            if (!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpGet("{id}/metrics/{evaluationExercise}/rougescore")]
        public IActionResult GetRougeScoreMetricsAsync([FromRoute] Guid id, [FromRoute] string evaluationExercise, [FromQuery]string rougeType)
        {
            if (!ChatCompletionExcerciseType.IsValid(evaluationExercise))
            {
                return BadRequest($"Invalid evaluation exercise {evaluationExercise}");
            }

            var metrics = _performanceMetricsService.FetchEvaluationSessionRougeScoreMetrics(id, evaluationExercise.ToLower(), rougeType);
            
            return Ok(metrics);
        }

        [HttpPost("{id}/metrics/{evaluationExercise}/bleuscore")]
        public async Task<IActionResult> UpsertBleuScoreMetricsAsync([FromRoute] Guid id, [FromRoute] string evaluationExercise, [FromBody] BleuScoreBatchEvaluationModel inputModel, CancellationToken cancellationToken)
        {
            if (inputModel == null)
            {
                return BadRequest("Input model is required");
            }

            if (!ChatCompletionExcerciseType.IsValid(evaluationExercise))
            {
                return BadRequest($"Invalid evaluation exercise {evaluationExercise}");
            }

            var command = new UpsertBleuBatchEvaluationRequest
            {
                SessionId = id,
                EvaluationExercise = evaluationExercise.ToLower(),
                BatchNumber = inputModel.BatchNumber,
                BleuScore = inputModel.BleuScore
            };            

            var upsertResult = await _performanceMetricsService.UpsertBleuBatchEvaluationAsync(command, cancellationToken);

            if (!upsertResult.IsSuccess)
            {
                return BadRequest(upsertResult);
            }

            return Ok(upsertResult);
        }

        [HttpGet("{id}/metrics/{evaluationExercise}/bleuscore")]
        public IActionResult GetBleuScoreMetrics([FromRoute] Guid id, [FromRoute] string evaluationExercise)
        {
            if (!ChatCompletionExcerciseType.IsValid(evaluationExercise))
            {
                return BadRequest($"Invalid evaluation exercise {evaluationExercise}");
            }

            var metrics = _performanceMetricsService.FetchEvaluationSessionBleuMetrics(id, evaluationExercise.ToLower());

            return Ok(metrics);
        }
    }
}