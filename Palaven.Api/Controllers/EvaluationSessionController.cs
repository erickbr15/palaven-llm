using Microsoft.AspNetCore.Mvc;
using Palaven.Application.Abstractions.DatasetManagement;
using Palaven.Application.Abstractions.PerformanceMetrics;
using Palaven.Application.Model.PerformanceEvaluation;
using Palaven.Application.PerformanceEvaluation;
using Palaven.Model.Datasets;

namespace Palaven.Api.Controllers
{
    [Route("api/evaluationsession")]
    [ApiController]
    public class EvaluationSessionController : ControllerBase
    {
        private readonly IPerformanceEvaluationService _performanceEvaluationService;        
        private readonly IInstructionDatasetService _datasetInstructionService;

        public EvaluationSessionController(IPerformanceEvaluationService performanceEvaluationService, IPerformanceMetricsService performanceMetricsService, 
            IInstructionDatasetService datasetInstructionService)
        {
            _performanceEvaluationService = performanceEvaluationService ?? throw new ArgumentNullException(nameof(performanceEvaluationService));            
            _datasetInstructionService = datasetInstructionService ?? throw new ArgumentNullException(nameof(datasetInstructionService));
        }        

        [HttpGet("{id}", Name = "GetEvaluationSession")]
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

            return CreatedAtAction("GetEvaluationSession", new { id = createdEvaluationSession.SessionId }, createdEvaluationSession);
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
    }
}