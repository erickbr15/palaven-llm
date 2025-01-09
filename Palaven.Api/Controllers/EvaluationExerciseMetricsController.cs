using Microsoft.AspNetCore.Mvc;
using Palaven.Api.Model.EvaluationSession;
using Palaven.Application.Abstractions.PerformanceMetrics;
using Palaven.Application.Model.PerformanceMetrics;
using Palaven.Infrastructure.Model.PerformanceEvaluation;

namespace Palaven.Api.Controllers
{
    [Route("api/evaluationsession")]
    [ApiController]
    public class EvaluationExerciseMetricsController : ControllerBase
    {
        private readonly IPerformanceMetricsService _performanceMetricsService;

        public EvaluationExerciseMetricsController(IPerformanceMetricsService performanceMetricsService)
        {
            _performanceMetricsService = performanceMetricsService ?? throw new ArgumentNullException(nameof(performanceMetricsService));
        }

        [HttpPost("{id}/{evaluationExercise}/metrics/bertscore")]
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

        [HttpGet("{id}/{evaluationExercise}/metrics/bertscore")]
        public IActionResult GetBertScoreMetrics([FromRoute] Guid id, [FromRoute] string evaluationExercise)
        {
            if (!ChatCompletionExcerciseType.IsValid(evaluationExercise))
            {
                return BadRequest($"Invalid evaluation exercise {evaluationExercise}");
            }

            var metrics = _performanceMetricsService.FetchEvaluationSessionBertscoreMetrics(id, evaluationExercise.ToLower());

            return Ok(metrics);
        }

        [HttpPost("{id}/{evaluationExercise}/metrics/rougescore")]
        public async Task<IActionResult> UpsertRougeScoreMetricsAsync([FromRoute] Guid id, [FromRoute] string evaluationExercise, [FromBody] IList<RougeScoreBatchEvaluationModel> inputModel, CancellationToken cancellationToken)
        {
            if (inputModel == null || inputModel.Count == 0)
            {
                return BadRequest("The rouge score metrics are required");
            }

            if (!ChatCompletionExcerciseType.IsValid(evaluationExercise))
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

        [HttpGet("{id}/{evaluationExercise}/metrics/rougescore")]
        public IActionResult GetRougeScoreMetricsAsync([FromRoute] Guid id, [FromRoute] string evaluationExercise, [FromQuery] string rougeType)
        {
            if (!ChatCompletionExcerciseType.IsValid(evaluationExercise))
            {
                return BadRequest($"Invalid evaluation exercise {evaluationExercise}");
            }

            var metrics = _performanceMetricsService.FetchEvaluationSessionRougeScoreMetrics(id, evaluationExercise.ToLower(), rougeType);

            return Ok(metrics);
        }

        [HttpPost("{id}/{evaluationExercise}/metrics/bleuscore")]
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

        [HttpGet("{id}/{evaluationExercise}/metrics/bleuscore")]
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
