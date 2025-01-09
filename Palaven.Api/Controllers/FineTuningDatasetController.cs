using Microsoft.AspNetCore.Mvc;
using Palaven.Application.Abstractions.DatasetManagement;
using Palaven.Application.Model.DatasetManagement;
using Palaven.Common;
using System.Globalization;
using System.Text;

namespace Palaven.Api.Controllers;

[Route("api/datasets")]
[ApiController]
public class FineTuningDatasetController : ControllerBase
{
    private readonly IFineTuningDatasetService _fineTuningDatasetService;

    public FineTuningDatasetController(IFineTuningDatasetService fineTuningDatasetService)
    {
        _fineTuningDatasetService = fineTuningDatasetService ?? throw new ArgumentNullException(nameof(fineTuningDatasetService));
    }

    [HttpPost("{datasetId}/llm/{largeLanguageModel}/finetuning")]
    public async Task<IActionResult> GenerateFineTuningDatasetAsync([FromRoute]Guid datasetId, [FromRoute]string largeLanguageModel, CancellationToken cancellationToken)
    {
        if (!string.Equals(largeLanguageModel, "google-gemma", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest($"The LLM {largeLanguageModel} is not supported for this operation.");
        }

        var request = new CreateFineTuningDatasetRequest
        {
            DatasetId = datasetId,
            LargeLanguageModel = largeLanguageModel.Trim().ToLowerInvariant()
        };

        await _fineTuningDatasetService.CreateFineTuningPromptDatasetAsync(request, cancellationToken);

        return Ok();
    }


    [HttpGet("{datasetId}/llm/{largeLanguageModel}/finetuning")]
    public IActionResult FetchFineTuningDataset([FromRoute] Guid datasetId, [FromRoute] string largeLanguageModel)
    {
        if (!string.Equals(largeLanguageModel, "google-gemma", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest($"The LLM {largeLanguageModel} is not supported for this operation.");
        }

        var request = new QueryFineTuningDatasetRequest
        {
            DatasetId = datasetId,
            LargeLanguageModel = largeLanguageModel.Trim().ToLowerInvariant()
        };

        var queryResult = _fineTuningDatasetService.FetchFineTuningPromptDataset(request);
        if(queryResult.HasErrors)
        {
            return BadRequest(queryResult);
        }

        var fileContent = CsvUtility.GenerateCsv(queryResult.Value,
            new CsvConfiguration
            {
                HasHeaderRecord = false,
                CultureInfo = CultureInfo.InvariantCulture,
                Encoding = Encoding.UTF8
            });

        var fileName = $"finetuning-dataset-gemma7b-it.{DateTime.Now.ToString("ddMMyyyy.HHmmss")}.csv";
        using var stream = new MemoryStream(fileContent);
        FileResult fileResult = new FileContentResult(stream.ToArray(), "application/octet-stream")
        {
            FileDownloadName = fileName
        };          

        return fileResult;
    }
}
